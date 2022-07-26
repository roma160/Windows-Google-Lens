# Windows Google Lens

Project to make **unofficial** native port of mobile version of Google Lense onto Windows.

The concept is firstly presented by https://github.com/WhoIsJayD/Google-Lens-For-Windows

## The difficult part of project
### Snipping Tool usage

The basis of the project is the Snipping Tool, that allows user to send screenshot to the Google Lens.
To make it work the UWP APIs were used:

* How-to guide: https://stackoverflow.com/questions/33692898/is-it-possible-to-use-uwp-apis-in-a-wpf-app
* Docs: https://github.com/MicrosoftDocs/windows-uwp/blob/docs/windows-apps-src/launch-resume/launch-screen-snipping.md

Another thing is, in order to understand that the snipping has ended, is to check whether the clipboard contents have been updated:
https://stackoverflow.com/questions/621577/how-do-i-monitor-clipboard-changes-in-c

### Acrylic window style

Firstly it was planned to copy style from the [Moders Flyouts](https://modernflyouts-community.github.io/),
so that the [ModernWPF](https://github.com/Kinnara/ModernWpf) lib was installed.

But then to this part the idea of using Acrylic window style was added. As a result, after large number of tryal and errors,
the `AcrylicWindow` class was implemented. The operability was only tested on Windows 11,
it is designed to work also on Windows 10.