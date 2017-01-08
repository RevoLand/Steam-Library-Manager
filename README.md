<p align="center">
	<img src="/Extras/Logo/slm.png?raw=true" width="550px" height="280px" alt="Steam Library Manager" />
	<br />
	<a href="https://github.com/RevoLand/Steam-Library-Manager/releases/latest">
		<img src="https://img.shields.io/github/release/RevoLand/Steam-Library-Manager.svg?style=flat-square" alt="Latest Release">
	</a>
    <a href="https://github.com/RevoLand/Steam-Library-Manager/releases/latest">
        <img src="https://img.shields.io/github/downloads/RevoLand/Steam-Library-Manager/total.svg?style=flat-square" alt="Total Downloads">
    </a>
    <a href="https://raw.githubusercontent.com/RevoLand/Steam-Library-Manager/master/LICENSE">
        <img src="https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square" alt="License">
    </a>
    <a href="https://github.com/RevoLand/Steam-Library-Manager/issues">
        <img src="https://img.shields.io/github/issues/RevoLand/Steam-Library-Manager.svg?style=flat-square" alt="Issues">
    </a>
</p>

**Table of Contents**
- [FAQ](#faq)
    - [What is SLM used for?](#what-is-slm-used-for)
    - [What SLM can do for me?](#what-slm-can-do-for-me)
    - [Where can i get SLM from?](#where-can-i-get-slm-from-do-i-have-to-compile-it-myself)
    - [Do i have to pay for SLM?](#do-i-have-to-pay-for-slm)
    - [Can SLM contain a bug?](#can-slm-contain-a-bug)
    - [I have a different question!](#i-have-a-different-question)
- [Credits](#credits)

FAQ
===================
### What is SLM used for?
SLM is a tool created to help you with managing Steam libraries. Best case-scenario for SLM to use is having multiple libraries on Steam or storing backups.

### What SLM can do for me?
Main reason for SLM coded is moving your apps between libraries. SLM also moves partial downloads *(/downloading/ folder)* and workshop files for you. You can queue up the apps you want to move and leave it alone. You won't be bothered unless you have asked to, or tasks are done. You can also compress apps instead of moving to save some space, no special compress method usen. Can be easily reachen and editen without SLM.

SLM also supports a few things in context menu of apps like '*Subscribed Workshop Items*' which takes your SteamID64 (*which is public*) from *config.vdf* and opens Subscribed Workshop Items for selected apps, sorted by last updated. Won't you need that? Just simply remove or deactive it from Settings tab, it won't bother you anymore in context menu.

### Where can i get SLM from? Do i have to compile it myself?
You can get the pre-compiled exe under [*releases*](https://github.com/RevoLand/Steam-Library-Manager/releases) section of Github. You can also get the 'latest/beta' exe under *Binaries* folder in source tree, the exe under that path should be up-to-date with latest commit.

If you have security concerns or want to do changes to SLM, there is completely no extra needing anything else than [Visual Studio](https://www.visualstudio.com/tr/vs/visual-studio-2017-rc/) *(which is mandatory for C#)*. As soon as you get VS, you can start working on SLM!

### Do i have to pay for SLM?
Nope, not at all. SLM is completely free and even better; open-source licensed with [MIT license](https://en.wikipedia.org/wiki/MIT_License). Nothing but love is needed for SLM.

### Can SLM contain a bug?
Yes! SLM might be buggy or bugged for some setups. As i have zero-to-none chance to test SLM on every different setup, it may give hiccups at some point. 
Luckily Task Manager *(which is used to move apps between libraries)* is running on a different thread. Is any kind of exception happens during app movement, your old files won't be removen no matter what you've picked as option. Unlike [*Humans*](http://www.imdb.com/title/tt4122068/) or [*Westworld*](http://www.imdb.com/title/tt0475784/), SLM ***does not*** have any kind of AI to take control over and remove your files.

Short of it no matter what your files should be safe. Just contact me out *(highly appreciated)* with the exception you have received and should be fixed asap!

### I have a different question!
Sure, just contact me over [Steam](https://steamcommunity.com/id/RevoLand/), [Reddit](https://www.reddit.com/user/RevoLand/) or Skype *(revolutionland)* and i will respond your question asap.

Credits
===================
- [Mert Ercan](https://steamcommunity.com/id/RevoLand/) *(that's me!)*
- [Mobeeuz](https://steamcommunity.com/id/Mobeeuz)
    - SLM wouldn't get this far without him! 
    - Also give a second and check his awesome book! [A Blue Horizon (Ashrealm)](https://www.amazon.com/dp/0995190917/)
 
*Below this line is ordered alphabetically.*
- [Ceriaz](https://steamcommunity.com/id/ceriaz)
    - SLM knows how to deal with .vdf files better now!
- [MageMaster](https://steamcommunity.com/profiles/76561197998719155)
    - No more disappearing tiles!
- [RemiGC](https://github.com/RemiGC)
    - So i know the existence of Path.Combine!
- [white_ghost](https://steamcommunity.com/profiles/76561197991469081)
    - SLM knows how to deal with custom app names because of him!
- *And everyone else who downloaded & using SLM!*

