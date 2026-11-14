# Language support

WxKit is written in C# and designed for C# first. F# and Visual Basic can use any .NET library, but some C# features are awkward or impossible to use from them. This document describes the rules that keep WxKit usable from all three, and what each language gets.

## Rules for the public API

These rules apply to every public member of the `WxKit` namespace:

- Do not use `ref struct` types in public signatures, except in overloads that also have an alternative. Visual Basic cannot use them, and F# can only use them in limited ways. Where a `ReadOnlySpan<T>` overload helps C# performance, there is also an overload that takes an array or `string`.
- Do not use `required` members. Visual Basic cannot set them, and the compiler blocks the type.
- Do not use C# 14 extension members, such as extension properties. Neither F# nor Visual Basic can consume them yet. Ordinary extension methods are fine.
- Do not put a public API only on a static abstract interface member. F# consumes these with warnings, and Visual Basic cannot call them.
- Use standard `EventHandler<T>` events, whose argument types derive from `EventArgs`. Visual Basic's `WithEvents` and `Handles` depend on this pattern.
- Avoid names that differ only in case. Visual Basic is not case sensitive.
- Optional parameters are fine. All three languages support them.

These rules cost C# very little. The internals of WxKit can use any C# feature.

## C#

C# gets the whole API as described in [Managed API](managed-api.md). The samples and documentation are written in C# first.

## F#

F# can use the C# API directly:

```fsharp
open WxKit

Application.Run(fun () ->
    let frame = new Frame(null, "Hello")
    let button = new Button(frame, "&Greet")
    button.Click.Add(fun _ -> MessageBox.Show("Hello!", "Greeting") |> ignore)
    frame.Show() |> ignore)
```

Events appear as F# events, so `Add`, `Subscribe` and the `Observable` functions work on them.

The F# compiler does not accept tabs for indentation, so F# files are the one place WxKit indents with spaces. The `.editorconfig` gets a section for `*.{fs,fsi,fsx}` that sets `indent_style = space` and `indent_size = 4`.

Once the C# API settles, a small `WxKit.FSharp` package could make F# code more natural. For example, a computation expression for building sizer layouts, or functions that return `unit` where the C# method returns a value most callers ignore. Whether this package is worth making is an open question, and it is not part of the first milestones.

## Visual Basic

Visual Basic can use the C# API directly, within the rules above:

```vb
Imports WxKit

Module Program
	Private WithEvents greet As Button

	Sub Main()
		Application.Run(Sub()
			Dim frame As New Frame(Nothing, "Hello")
			greet = New Button(frame, "&Greet")
			frame.Show()
		End Sub)
	End Sub

	Private Sub OnGreet(sender As Object, e As CommandEvent) Handles greet.Click
		MessageBox.Show("Hello!", "Greeting")
	End Sub
End Module
```

Visual Basic gets no package of its own.

## Keeping it working

The F# and Visual Basic samples build in CI, so a change that breaks either language fails the build. Each sample uses a few features that are known to be fragile across languages, such as events, optional parameters and enum flags. The F# sample also publishes with Native AOT, since F# has its own trimming warnings to watch for.
