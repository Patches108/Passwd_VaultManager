# Passwd_VaultManager

I created this project to help myself learn WPF developement with C#. I learned a lot, and I can see where I could've done with with MVVM, but it was a learning experience. As such, the code isn't completely MVVM friendly.

Co-Authored by ChatGPT ^_^

----------------------------------------------------------------

Password Vault Manager is a lightweight Windows desktop application for securely storing and managing application credentials. Built with WPF and an MVVM architecture, it focuses on clarity, usability, and strong password practices without unnecessary complexity. 
The application allows users to create, edit, and organize vault entries containing application names, usernames, and passwords. Each entry includes real-time feedback on password strength (entropy/bitrate), optional character exclusions, and clear status indicators to ensure credentials are complete and secure.

Key Features:

Secure local password vault storage
Real-time password strength estimation
Search, filter, and sort vault entries
Clean MVVM-based WPF UI with custom styling
Optional Windows startup integration
System tray support with background operation
Configurable fonts, font sizes, and sound feedback
First-time helper guidance for new users
Atomic settings persistence to prevent data corruption#

Password Vault Manager is designed to be:

Simple – no cloud sync or external dependencies
Transparent – clear visual indicators for entry completeness
Safe – local-only storage and atomic file writes
Maintainable – clean separation of concerns and well-documented code


This project serves both as a practical credential manager and as a well-structured example of a modern WPF MVVM application.

See my coding journey (dev log):
https://www.youtube.com/watch?v=R5dIAbL2IdU
