# Favorites Asset Window

[![License](https://img.shields.io/badge/Licenses-MIT-brightgreen)](https://github.com/MasyoLab/UnityTools-FavoritesAsset/blob/master/LICENSE.md)
[![Unity Editor Extension Test](https://github.com/MasyoLab/UnityTools-FavoritesAsset/actions/workflows/unity-test.yml/badge.svg)](https://github.com/MasyoLab/UnityTools-FavoritesAsset/actions/workflows/unity-test.yml)
[![Unity 2019.4+](https://img.shields.io/badge/Unity-2019.4%2B-blue)](https://unity.com/releases/editor/archive)

[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FMasyoLab%2FUnityTools-FavoritesAsset.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2FMasyoLab%2FUnityTools-FavoritesAsset?ref=badge_shield)

| English | [日本語](README_ja.md) |
| :------: | :---- |

## Overview

This function allows you to bookmark (register as a favorite) resources.

![main](https://user-images.githubusercontent.com/20793765/129486046-a0e18e0d-a60f-4532-a188-be63b02f8781.png)

## Install

### PackageManager

![user action 7](https://user-images.githubusercontent.com/20793765/123511981-bb09a080-d6bf-11eb-8a15-68bad60e0278.png)

In the package manager, press “Add package from git URL...” and paste the following string into the input field that comes up.

```text
https://github.com/MasyoLab/UnityTools-FavoritesAsset.git?path=Assets/MasyoLab/FavoritesAsset
```

### UnityPackage

It is available from [Releases](https://github.com/MasyoLab/UnityTools-FavoritesAsset/releases).

### Usage

![Open the editor](https://user-images.githubusercontent.com/20793765/126061572-bb8e0619-52fa-4560-b5b7-50f51db3e675.png)

The extended editor can be used from “Tools”.

Note : The access method may change with future expansion.

### Add to Favorites

You can bookmark assets by dragging and dropping them.

![fav assets action 1](https://user-images.githubusercontent.com/20793765/129484724-dda7322e-3049-4762-996c-79ea88906584.gif)

### Ping asset. Open asset

Registered assets can have a focus button for the Project view and an open button for the asset.

![fav assets action 2](https://user-images.githubusercontent.com/20793765/128969150-67e8dfe0-786b-457c-bcd1-4b40dc0bcc1d.gif)

### Bookmark suffixes and colors (PATATAJEC fork)

Click the pencil icon next to **×** on any bookmark to edit its appearance:

- **Name suffix** adds a note after the original asset name, e.g. `Materials — Character`.
- **Color code** adds a colored strip and a subtle tint to the bookmark button. Pick a quick color or use Unity's color picker.
- **Save** applies and saves the changes; **Cancel** discards them. **Reset**, followed by **Save**, restores the original label and default appearance.

These settings belong to each bookmark, so the same asset can have different notes and colors in different groups. They do not rename or modify the asset or folder. Hover over the asset button to see its full project path.

Suffixes and colors persist in the existing favorites files and are included in import/export and group copies. Existing favorites without appearance settings keep their original labels and styling. Copying a group creates independent bookmark entries.

To install this fork from **Window > Package Manager > + > Add package from git URL...**, use:

```text
https://github.com/PATATAJEC/UnityTools-FavoritesAsset.git?path=Assets/MasyoLab/FavoritesAsset
```

EditMode regression tests cover older save files, appearance serialization and export/import, renamed/moved folders and prefabs, independent group copies, and Unity's edit icon. The test project's manifest includes Unity Test Framework so the tests can also run locally.

### Menu commands and separators (PATATAJEC fork, 1.7.0)

Use the **+ Add** dropdown above the bookmark list:

- **Menu command...** adds a bookmark using an exact Unity menu path, for example `Assets/Show in Explorer` on Windows. The final part of the path becomes its default name. You can add a suffix and a color, and edit the path later with the pencil button. Saving never executes the command.
- **Separator** inserts an empty spacer row without a horizontal line. Drag its handle to position it between any bookmarks, edit it with the pencil, or remove it with **×**. A separator is a visual divider; it does not move other entries as a section.

Click a command bookmark to execute it with Unity's current selection. A command may need a selected asset or a particular editor state. If its path is missing or unavailable, the plugin displays a message. Enter the menu hierarchy with `/` separators, preserving spelling and case; the path is not an asset file path. Commands provided by a package require that package to remain installed.

Commands, separators, their order, and command colors/suffixes persist in the same group files and are included in import/export and independent group copies. Existing asset bookmarks continue to load normally. Separators are excluded from the favorite count.

In **1.8.0**, separator selection is shown only at the left and right edges instead of filling the whole row. The pencil opens **Edit separator**, where **Caption (optional)** adds a section heading and **Color code** adds a subtle tint and a colored side accent. Leave the caption empty and color disabled for a plain blank spacer. **Reset**, then **Save**, restores that blank appearance. Separator captions and colors are also saved, exported, and copied with groups.

### Unlock favorites

![fav assets action 3](https://user-images.githubusercontent.com/20793765/129485099-4ff09124-6fce-4e92-b19b-4126705b7295.gif)

### Sort Favorites

![fav assets action 4](https://user-images.githubusercontent.com/20793765/129485130-48589e20-bcfd-4566-9fb1-8129edb0fbc5.gif)

### Add a favorite group

![group1](https://user-images.githubusercontent.com/20793765/129485493-c3330196-2e44-49e8-90ca-e27f73f7a7da.gif)

### Sorting favorite groups

![group2](https://user-images.githubusercontent.com/20793765/129485503-44fa42e1-ed14-49cb-8e88-71eb6a986da1.gif)

### Deleting a favorite group

When you delete a bookmark, the contents of the bookmark are also deleted.

![group3](https://user-images.githubusercontent.com/20793765/129485516-11db3948-205c-4e42-a047-d254f96d1bba.gif)

### menu

![menu](https://user-images.githubusercontent.com/20793765/129485564-7c825a2e-f8b0-4cba-9bbd-a18b1411ef49.png)

The following data is included in the import/export target.

- Favorite Data
- Favorite Groups
- Last Favorite Group Opened

## Supported Unity Versions

This editor extension has been tested on the following Unity LTS versions:

- ✅ Unity 6000.3 LTS
- ✅ Unity 6000.0 LTS
- ✅ Unity 2022.3 LTS
- ✅ Unity 2021.3 LTS
- ✅ Unity 2020.3 LTS
- ✅ Unity 2019.4 LTS

> Automated tests run on all versions when a Pull Request is created via CI/CD. For details, see the [CI/CD Setup Guide](.github/CI_SETUP.md).

## Other

This extension is a tool I created as a hobby.

Regarding feature additions, I will work on them if I have good ideas and motivation.  
If you have any ideas, pull requests for feature extensions are very welcome.

Regarding bug fixes that have been reported, I will fix them as quickly as 1 week, or as late as 3 weeks.

## License

This extension editor is licensed under the MIT License. Please see [LICENSE](https://github.com/MasyoLab/UnityTools-FavoritesAsset/blob/master/LICENSE.md) for more information.
