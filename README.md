# Monitored Undo Framework
Monitored Undo is a .NET Standard 2.0 Undo / Redo framework. The framework "monitors" changes 
to the model, keeps a history of undo / redo operations, and assists with applying an undo 
back to the model.

# Quick Start...
To get a quick idea of how it works, check out the unit tests and the sample model classes.

# Documentation
Please refer to the [documentation folder](docs/) in the repo for details on usage.

# Sample Application...
There is a WPF Sample application in the source code tree. This is a very simple app that 
shows a couple features of the undo framework. It does not follow best practices, but does
illustrate how to hook things up.

# Reference Application...
For a more complete example application, consider the 
[Photo Tagger reference app](https://nathan.alner.net/2010/10/13/wpf-amp-entity-framework-4-tales-from-the-trenches/) 
sample application. It was created for a presentation related to WPF applications, EF, and undo.

# Now on NuGet...
MUF is [available on NuGet](http://nuget.org/List/Packages/MUF).
