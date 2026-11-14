using System.Runtime.CompilerServices;

// Every type that crosses into the shim must be blittable, so a signature that would need the
// runtime to marshal it fails to compile instead of needing a stub at run time.
[assembly: DisableRuntimeMarshalling]
