# XIVComboPlugin
This plugin condenses combos and mutually exclusive abilities onto a single button. Thanks to Meli for the initial start, and obviously goat for making any of this possible.

## About
XIVCombo is a plugin to allow for "one-button" combo chains, as well as implementing various other mutually-exclusive button consolidation and quality of life replacements. Some examples of the functionality it provides:
* ~~All~~ Most weaponskill combos are put onto a single button (sorry DNC and MNK!).
* Enochian changes into Fire 4 and Blizzard 4 depending on your stacks, a la PvP Enochian.
* Hypercharge turns into Heat Blast while Overheated.
* Jump/High Jump turns into Mirage Dive while Dive Ready.
* And many, many more!
For some jobs, this frees a massive amount of hotbar space (looking at you, DRG). For most, it removes a lot of mindless tedium associated with having to press various buttons that have little logical reason to be separate.

## Installation
* Download the zip file from the most recent release.
* Unzip said zip file inside of `%appdata%/XIVLauncher/installedPlugins`.
  * NOTE: %appdata% refers to the ROAMING folder inside of the AppData folder.
    *If you want to get there instantly, press Windows key + r, and type %appdata% in the box that pops up.
## In-game usage
* Type `/pcombo` to pull up a GUI for editing active combo replacements.
* Drag the named ability from your ability list onto your hotbar to use.
  * For example, to use DRK's Souleater combo, first check the box, then place Souleater on your bar. It should automatically turn into Hard Slash.
  * The description associated with each combo should be enough to tell you which ability needs to be placed on the hotbar.
  * Make sure you press "Save and close". Don't just X out.
### Examples
![](https://github.com/attickdoor/xivcomboplugin/raw/master/res/souleater_combo.gif)
![](https://github.com/attickdoor/xivcomboplugin/raw/master/res/hypercharge_heat_blast.gif)
![](https://github.com/attickdoor/xivcomboplugin/raw/master/res/eno_swap.gif)

## Known Issues
* There is an issue with XIVLauncher not saving configurations when the game is closed and relaunched. Until this has been resolved, you can type /pcombo setall to quickly set all combos when the game is restarted
