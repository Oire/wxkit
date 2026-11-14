# Testing

Testing GUI code is harder than testing most libraries. The code needs a display, an event loop, and, on macOS, the process's main thread. This document describes how WxKit gets tests running anyway.

## Kinds of tests

WxKit has three kinds of tests:

- Generator tests, in `tests/WxKit.Generator.Tests`. These check that the generator turns sample Doxygen XML into the expected output. They are plain unit tests with no GUI, and they run anywhere.
- Managed API tests, in `tests/WxKit.Tests`. These create real windows and check lifetime, events, threading, strings, and the rest of the managed API against real wxWidgets.
- Native AOT smoke tests. CI publishes the samples with Native AOT and runs each one in a mode that shows a window, waits for the event loop to start, and exits with a success code.

## The main thread problem

Test frameworks such as xUnit run tests on worker threads. wxWidgets needs every GUI call on one thread, and on macOS that thread must be the process's main thread. A test cannot simply create a frame.

The proposal is for `WxKit.Tests` to be its own executable with its own `Main`. It starts wxWidgets on the main thread, runs every test from inside the event loop, and reports results in a format CI understands. Each test can pump the event loop to let events happen, for example to check that closing a frame raises `Closing` and then destroys it. wxWidgets's own test suite works the same way.

WxKit uses xUnit v3, whose test projects are already executables. The test project replaces xUnit's generated entry point with its own, which starts wxWidgets on the main thread and has xUnit hand each test to it. This keeps `dotnet test` and the usual tooling. If xUnit cannot be made to run tests on the main thread on macOS, the fallback is a small test runner of WxKit's own.

## Displays in CI

- Windows runners have a desktop, so tests run as they are.
- macOS runners have a window server, so tests run as they are.
- Linux runners have no display. Tests run under `xvfb-run`, which provides a virtual X server, with GTK set to use X11.

## What tests avoid

- Tests do not depend on the pixel layout of native controls, which differs between platforms, themes, and DPI settings.
- Tests do not show modal dialogs, which would block the run. Dialog code is tested by creating dialogs and inspecting them without calling `ShowModal`.
- A hung test must not hold a runner for hours. The test job has a timeout, and the runner can also stop a test that pumps the event loop for too long.

## Accessibility checks

Native controls are accessible by default, but the way an application builds them can still break that, such as creating a label after the control it names. Later, the tests can check WxKit's samples with the platform accessibility APIs, such as UI Automation on Windows, to confirm that controls have the names and roles expected. This is not part of the first milestones.
