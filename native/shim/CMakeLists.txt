cmake_minimum_required(VERSION 3.25)
project(wxkit LANGUAGES C CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# wxWidgets is always linked statically: into the application by Native AOT, or into wxkit's own
# shared library otherwise. Its options are forced here, rather than passed by the build script, so
# the generator can rely on exactly this set of features.
set(wxBUILD_SHARED OFF CACHE BOOL "" FORCE)
set(wxBUILD_MONOLITHIC OFF CACHE BOOL "" FORCE)
set(wxBUILD_SAMPLES OFF CACHE STRING "" FORCE)
set(wxBUILD_TESTS OFF CACHE STRING "" FORCE)
set(wxBUILD_DEMOS OFF CACHE BOOL "" FORCE)
set(wxBUILD_BENCHMARKS OFF CACHE BOOL "" FORCE)
set(wxBUILD_INSTALL OFF CACHE BOOL "" FORCE)
set(wxBUILD_COMPATIBILITY NONE CACHE STRING "" FORCE)
# Native AOT links the static C runtime, so everything it links has to agree with it.
set(wxBUILD_USE_STATIC_RUNTIME ON CACHE BOOL "" FORCE)
set(wxUSE_ACCESSIBILITY ON CACHE BOOL "" FORCE)
# The Edge web view downloads its SDK from NuGet while configuring. It comes back with the web view itself.
set(wxUSE_WEBVIEW OFF CACHE BOOL "" FORCE)
set(wxUSE_WEBVIEW_EDGE OFF CACHE BOOL "" FORCE)
if(NOT DEFINED wxBUILD_DEBUG_LEVEL)
	set(wxBUILD_DEBUG_LEVEL 0 CACHE STRING "" FORCE)
endif()

add_subdirectory(../wxWidgets wx EXCLUDE_FROM_ALL)

set(WXK_SOURCES
	generated/generated.cpp
	src/app.cpp
	src/clipboard.cpp
	src/event.cpp
	src/handlers/all.cpp
	src/handlers/png.cpp
	src/handlers/jpeg.cpp
	src/handlers/tiff.cpp
	src/handlers/gif.cpp
	src/handlers/webp.cpp
	src/handlers/pnm.cpp
	src/handlers/pcx.cpp
	src/handlers/iff.cpp
	src/handlers/tga.cpp
	src/handlers/xpm.cpp
	src/handlers/icon.cpp
	src/menu.cpp
	src/object.cpp
	src/sizer.cpp
	src/string.cpp
	src/timer.cpp
	src/validator.cpp
	src/window.cpp
)

# The static library and the shared one are built from the same sources, and end up as
# wxkitnative.lib and wxkitnative.dll in directories of their own. The name is not wxkit because
# Windows file names ignore case, and wxkit.dll would collide with the managed WxKit.dll.
add_library(wxkitnative_static STATIC ${WXK_SOURCES})
target_include_directories(wxkitnative_static PUBLIC include)
target_link_libraries(wxkitnative_static PUBLIC wx::core wx::base)
set_target_properties(wxkitnative_static PROPERTIES
	OUTPUT_NAME wxkitnative
	ARCHIVE_OUTPUT_DIRECTORY $<1:${CMAKE_BINARY_DIR}/static>
)

add_library(wxkitnative SHARED ${WXK_SOURCES})
target_include_directories(wxkitnative PUBLIC include)
target_compile_definitions(wxkitnative PRIVATE WXK_BUILDING_SHARED)
target_link_libraries(wxkitnative PRIVATE wx::core wx::base)
set_target_properties(wxkitnative PROPERTIES
	RUNTIME_OUTPUT_DIRECTORY $<1:${CMAKE_BINARY_DIR}/shared>
	ARCHIVE_OUTPUT_DIRECTORY $<1:${CMAKE_BINARY_DIR}/shared>
)

if(WIN32)
	# wxMSW loads its cursors and bitmaps from the module it runs in. The shared library carries them
	# itself. A Native AOT application gets them as a .res file to link, which this object library makes.
	target_sources(wxkitnative PRIVATE src/wxkit.rc)
	add_library(wxkit_resources OBJECT src/wxkit.rc)
	target_link_libraries(wxkit_resources PRIVATE wx::core)
endif()
