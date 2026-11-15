# WxKit native library

wxWidgets and the native shim of [WxKit](https://www.nuget.org/packages/WxKit), the .NET bindings to wxWidgets, for one runtime. Reference `WxKit`, which brings in this package for each runtime it supports, rather than this package itself.

It holds:

- `wxkitnative`, a shared library with wxWidgets linked into it, which an application loads when it runs as it is.
- The static libraries of wxWidgets and the shim, and the targets that link them, which a Native AOT publish links into the executable, so nothing ships beside it.
- `THIRD-PARTY-NOTICES.md`, which lists wxWidgets and the libraries it builds in, such as zlib and libpng, with their licenses, and each license's text, in the `licenses` folder.
