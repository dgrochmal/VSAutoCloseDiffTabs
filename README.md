# Auto Close Diff Tabs

A lightweight Visual Studio extension that automatically closes Git diff tabs when you navigate away from them, keeping your tab bar clean and clutter-free.

[![Visual Studio Marketplace](https://img.shields.io/visual-studio-marketplace/v/DanGrochmal.autoclosedifftabs?label=VS%20Marketplace)](https://marketplace.visualstudio.com/items?itemName=DanGrochmal.autoclosedifftabs)
[![Installs](https://img.shields.io/visual-studio-marketplace/i/DanGrochmal.autoclosedifftabs)](https://marketplace.visualstudio.com/items?itemName=DanGrochmal.autoclosedifftabs)
[![Rating](https://img.shields.io/visual-studio-marketplace/r/DanGrochmal.autoclosedifftabs)](https://marketplace.visualstudio.com/items?itemName=DanGrochmal.autoclosedifftabs)


## The Problem

When reviewing Git changes in Visual Studio, diff tabs accumulate quickly. After checking staged changes, unstaged changes, or comparing commits, you're left with a cluttered tab bar full of read-only diff views that you have to close manually.

## The Solution

**Auto Close Diff Tabs** automatically closes Git diff tabs the moment you switch to another document. No configuration needed—it just works.

## Features

- **Zero configuration** – Works instantly after installation
- **Automatic cleanup** – Diff tabs close as soon as you switch to another document
- **Lightweight** – Minimal footprint with async background loading
- **Smart detection** – Identifies diff windows via:
  - Document moniker (GitDiff:// URIs)
  - Window caption patterns ("Diff -", "vs", "Staged Changes", etc.)
  - Editor type GUID (Visual Studio's built-in diff editor)

## Installation

**[Install from Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=DanGrochmal.autoclosedifftabs)**

Or from within Visual Studio:

### Manual Installation

1. Download the `.vsix` file from the [Releases](../../releases) page or [Marketplace](https://marketplace.visualstudio.com/items?itemName=DanGrochmal.autoclosedifftabs)
2. Double-click to install
3. Restart Visual Studio

## How It Works

1. You open a Git diff (staged changes, unstaged changes, file comparison, etc.)
2. The extension tracks the diff window
3. When you click on any other file or tab, the diff is automatically closed
4. Diff views are closed without prompting to save (they're read-only comparisons)

## Requirements

- Visual Studio 2022 (version 17.0 or later)
- 64-bit (amd64) architecture

## FAQ

**Q: Will this close my regular file tabs?**  
A: No. The extension only targets Git diff windows, not regular source files.

**Q: What if I want to keep a diff open?**  
A: Currently, all diff tabs auto-close when losing focus. A future version may add pinning or configuration options.

**Q: Does this work with other diff tools?**  
A: It's designed for Visual Studio's built-in Git diff viewer. Third-party diff tools may or may not be detected.

## Contributing

Contributions are welcome! Feel free to open issues or submit pull requests.

## License

[MIT License](LICENSE)

## Author

Dan Grochmal
