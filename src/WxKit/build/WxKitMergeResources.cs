#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

// The linker takes only one set of Win32 resources, so a Native AOT application cannot have both its
// own, which hold its icon, version, and manifest, and the ones wxWidgets needs, such as its
// cursors. This task reads the application's resources out of its managed assembly, the same ones
// Native AOT would copy, and writes them to one .res file together with WxKit's. The application's
// resources win wherever both have one with the same type, name, and language.
public sealed class WxKitMergeResources : Task {
	[Required]
	public string Assembly { get; set; } = string.Empty;

	public ITaskItem[] Resources { get; set; } = Array.Empty<ITaskItem>();

	[Required]
	public string Output { get; set; } = string.Empty;

	public override bool Execute() {
		try {
			List<Resource> merged = ReadAssembly(File.ReadAllBytes(Assembly));
			HashSet<string> seen = new();
			foreach (Resource resource in merged) seen.Add(resource.Key);
			foreach (ITaskItem item in Resources) {
				foreach (Resource resource in ReadResFile(File.ReadAllBytes(item.ItemSpec))) {
					if (seen.Add(resource.Key)) merged.Add(resource);
				}
			}
			Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(Output))!);
			using FileStream stream = File.Create(Output);
			using BinaryWriter writer = new(stream);
			// A .res file starts with an empty entry, which tells tools it is 32 bit.
			WriteEntry(writer, new Resource(new ResourceName(0), new ResourceName(0), 0, 0, Array.Empty<byte>()));
			foreach (Resource resource in merged) WriteEntry(writer, resource);
			return true;
		} catch (Exception error) when (error is IOException or InvalidDataException or UnauthorizedAccessException) {
			Log.LogError($"WxKit could not merge the application's Win32 resources with its own: {error.Message}");
			return false;
		}
	}

	// A PE file keeps its resources as a tree of directories: type, then name, then language.
	private static List<Resource> ReadAssembly(byte[] image) {
		List<Resource> resources = new();
		int header = BitConverter.ToInt32(image, 0x3C);
		if (BitConverter.ToUInt32(image, header) != 0x00004550) throw new InvalidDataException("The assembly is not a PE file.");
		int sectionCount = BitConverter.ToUInt16(image, header + 6);
		int optionalSize = BitConverter.ToUInt16(image, header + 20);
		int optional = header + 24;
		bool isPe32Plus = BitConverter.ToUInt16(image, optional) == 0x20B;
		int directories = optional + (isPe32Plus ? 112 : 96);
		uint resourceRva = BitConverter.ToUInt32(image, directories + 2 * 8);
		if (resourceRva == 0) return resources;
		int sections = optional + optionalSize;
		int Offset(uint rva) {
			for (int index = 0; index < sectionCount; index++) {
				int section = sections + index * 40;
				uint virtualSize = BitConverter.ToUInt32(image, section + 8);
				uint virtualAddress = BitConverter.ToUInt32(image, section + 12);
				uint rawSize = BitConverter.ToUInt32(image, section + 16);
				uint rawPointer = BitConverter.ToUInt32(image, section + 20);
				if (rva >= virtualAddress && rva < virtualAddress + Math.Max(virtualSize, rawSize)) return checked((int)(rva - virtualAddress + rawPointer));
			}
			throw new InvalidDataException("A resource lies outside every section.");
		}
		int root = Offset(resourceRva);
		foreach ((ResourceName type, int typeDirectory) in ReadDirectory(image, root, root)) {
			foreach ((ResourceName name, int nameDirectory) in ReadDirectory(image, root, root + typeDirectory)) {
				foreach ((ResourceName language, int dataEntry) in ReadDirectory(image, root, root + nameDirectory)) {
					int entry = root + dataEntry;
					uint dataRva = BitConverter.ToUInt32(image, entry);
					int size = BitConverter.ToInt32(image, entry + 4);
					byte[] data = new byte[size];
					Buffer.BlockCopy(image, Offset(dataRva), data, 0, size);
					resources.Add(new Resource(type, name, (ushort)language.Id, 0x1030, data));
				}
			}
		}
		return resources;
	}

	// Each entry's offset is relative to the start of the resource section, with the top bit set
	// when it leads to another directory rather than to data.
	private static IEnumerable<(ResourceName Name, int Offset)> ReadDirectory(byte[] image, int root, int directory) {
		int count = BitConverter.ToUInt16(image, directory + 12) + BitConverter.ToUInt16(image, directory + 14);
		for (int index = 0; index < count; index++) {
			int entry = directory + 16 + index * 8;
			uint name = BitConverter.ToUInt32(image, entry);
			uint offset = BitConverter.ToUInt32(image, entry + 4) & 0x7FFFFFFF;
			if ((name & 0x80000000) == 0) {
				yield return (new ResourceName((ushort)name), (int)offset);
			} else {
				int text = root + (int)(name & 0x7FFFFFFF);
				int length = BitConverter.ToUInt16(image, text);
				yield return (new ResourceName(Encoding.Unicode.GetString(image, text + 2, length * 2)), (int)offset);
			}
		}
	}

	private static List<Resource> ReadResFile(byte[] file) {
		List<Resource> resources = new();
		int position = 0;
		while (position + 8 <= file.Length) {
			int dataSize = BitConverter.ToInt32(file, position);
			int headerSize = BitConverter.ToInt32(file, position + 4);
			int cursor = position + 8;
			ResourceName type = ReadResName(file, ref cursor);
			ResourceName name = ReadResName(file, ref cursor);
			cursor = Align(cursor);
			ushort flags = BitConverter.ToUInt16(file, cursor + 4);
			ushort language = BitConverter.ToUInt16(file, cursor + 6);
			byte[] data = new byte[dataSize];
			Buffer.BlockCopy(file, position + headerSize, data, 0, dataSize);
			// The empty entry that starts the file is not a resource.
			if (!(dataSize == 0 && type.Name is null && type.Id == 0)) resources.Add(new Resource(type, name, language, flags, data));
			position = Align(position + headerSize + dataSize);
		}
		return resources;
	}

	private static ResourceName ReadResName(byte[] file, ref int cursor) {
		if (BitConverter.ToUInt16(file, cursor) == 0xFFFF) {
			ushort id = BitConverter.ToUInt16(file, cursor + 2);
			cursor += 4;
			return new ResourceName(id);
		}
		int start = cursor;
		while (BitConverter.ToUInt16(file, cursor) != 0) cursor += 2;
		string text = Encoding.Unicode.GetString(file, start, cursor - start);
		cursor += 2;
		return new ResourceName(text);
	}

	private static void WriteEntry(BinaryWriter writer, Resource resource) {
		using MemoryStream header = new();
		using (BinaryWriter names = new(header, Encoding.Unicode, leaveOpen: true)) {
			WriteResName(names, resource.Type);
			WriteResName(names, resource.Name);
			while (header.Length % 4 != 0) names.Write((byte)0);
			names.Write(0u);
			names.Write(resource.Flags);
			names.Write(resource.Language);
			names.Write(0u);
			names.Write(0u);
		}
		writer.Write(resource.Data.Length);
		writer.Write(checked((int)header.Length + 8));
		writer.Write(header.ToArray());
		writer.Write(resource.Data);
		while (writer.BaseStream.Position % 4 != 0) writer.Write((byte)0);
	}

	private static void WriteResName(BinaryWriter writer, ResourceName name) {
		if (name.Name is null) {
			writer.Write((ushort)0xFFFF);
			writer.Write(name.Id);
		} else {
			writer.Write(Encoding.Unicode.GetBytes(name.Name));
			writer.Write((ushort)0);
		}
	}

	private static int Align(int value) => (value + 3) & ~3;

	// A resource's type and name are each either a number or a string.
	private readonly struct ResourceName {
		public ResourceName(ushort id) {
			Id = id;
			Name = null;
		}

		public ResourceName(string name) {
			Id = 0;
			Name = name;
		}

		public ushort Id { get; }

		public string? Name { get; }

		public override string ToString() => Name is null ? "#" + Id : Name.ToUpperInvariant();
	}

	private sealed class Resource {
		public Resource(ResourceName type, ResourceName name, ushort language, ushort flags, byte[] data) {
			Type = type;
			Name = name;
			Language = language;
			Flags = flags;
			Data = data;
		}

		public ResourceName Type { get; }

		public ResourceName Name { get; }

		public ushort Language { get; }

		public ushort Flags { get; }

		public byte[] Data { get; }

		public string Key => $"{Type}/{Name}/{Language}";
	}
}
