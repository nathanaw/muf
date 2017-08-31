# Monitored Undo Framework
Monitored Undo is an Undo / Redo framework that makes it simple for .NET developers to add 
Undo capabilities to an application. The framework "monitors" changes to the model, keeps a 
history of undo / redo operations, and assists with applying an undo back to the model.

# Quick Start...
To get a quick idea of how it works, check out the unit tests and the sample model classes.

# Documentation
Please refer to the [documentation folder](docs/) in the repo for details on usage.

# Sample Application...
There is a WPF Sample application in the source code tree. This is a very simple app that 
shows a couple features of the undo framework. It does not follow best practices, but does
illustrate how to hook things up.

# Reference Application...
For a more complete example application, please check out my Photo Tagger app. I created 
this for a .NET Special Interest Group presentation. It covers my "WPF Tales from the 
Trenches", including Undo. You can find the 
[Photo Tagger reference app](http://blog.alner.net/archive/2010/10/13/wpf_ef_4_sig_presentation_2010.aspx) 
on my [blog](http://blog.alner.net).

# Now on NuGet...
MUF is available on NuGet. You can view details on the 
[NuGet Gallery](http://nuget.org/List/Packages/MUF) or search for it in the Add Library 
Reference... dialog via Visual Studio. 

For you command line types, it's on NuGet as "MUF".  

`PM> Install-Package MUF`
