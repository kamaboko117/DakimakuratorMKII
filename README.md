<p align="center">
  <img height="200" src="https://github.com/kamaboko117/DakimakuratorMKII/blob/main/Packages/fr.kmbk.dakimakurator/Runtime/textures/DAKIMAKURATORMKII.png">
</p> 
<p align="center">
A dakimakura generator for VRC with player scan textures
</p>
<p align="center">
<a href="https://kamaboko117.github.io/VPMListing/">Add to VRChat's Creator Companion</a>
</p>

---

[![Watch the video](https://cdn.discordapp.com/attachments/1036599465429172244/1441206915823566968/SFSFDSFDSFSDFDS.png?ex=6920f409&is=691fa289&hm=e8156bf3bac1ea895b335c6ad515e0537a30fb892fb6e98e3ceff637fabbbf82&)](https://www.youtube.com/watch?v=v6Q7heUiMVQ)

---

## ▶ Getting Started

* Add Dakimakurator MKII from the [listing](https://kamaboko117.github.io/VPMListing/)
* From VRChat's Creator Companion, add the package to yout project
* In Unity, you will find the prefab in `Packages/fr.kmbk.dakimakurator/Runtime` (you can follow the video "tutorial" above)
  * If you don't have Text Mesh Pro yet, you need to install it => place the prefab, in the hierarchy, open it, open `Scanner`, look for `UI`, enable it, it should show the TMP install modal. Once you're done, you can disable `UI` again.
  * Sometimes, the sdk is weird and will tell you the dakimakuras have empty scripts or what not. in such cases => open the prefab in the hierarchy again, open `Pillow Pool`, then click through each pillow (you can click on the first one and then use the `down arrow`). This is not a joke btw, this is real SDK Behavior.
* Profit


## PLEASE CONTRIBUTE IF YOU CAN

I'm not really good at Unity, VRC scripting, 3D modeling... anything needed to make this prefab basically, so if you want to improve it PLEASE DO OPEN A PULL REQUEST!!

if you find a bug, please report it, I'll try and fix what I can


## Known Limitations

* Late joiners will not be able to sync their textures, it's not really possible to have textures as synced variables (I think?)
* People will see whatever they see avatars as, if you block someone's avatar and they scan themselves, their avatar will not appear on the pillow, if someone's avatar is showing as an impostor, then you will also see an impostor on the texture, etc...
* If you want to be able to burn the pillows, you need to setup a trigger on layer 23 (you might need to create this layer first). It doesn't matter how you name it, as long as it's layer 23, if you want to change this you'll have to modify the script directly.
* 20 pillows max ! (you can change this if you add more pillows to the pool)
* One person at a time ! (Or not but this might give you an unexpected texture)
* Dakimakurator MKII is not responsible for any harm occuring while operating the scanner, smelling pillows, scanning while wearing metallic objects. The pillows are very flammable, please be careful 

## How much MONEYZ

This package is free and distributed under a CC-BY-NC license. For more info, check the license directly. Please credit me with a link to the package. If you have an UI at the entrance of your world, that's the perfect place to credit the package (and other packages/assets that you used). I give this for free so that anyone can use it, but if you DO have money, consider a donation on [Buy me a Coffee](buymeacoffee.com/kamaboko). Any amount that you think is fair.

DO NOT SELL THIS PACKAGE, I WILL FIND YOU, THIS IS NOT A JOKE

## Customizing the prefab

If you know what you're doing, feel free to customize every aspect of the prefab (voicelines, 3D meshes, UI...). Just make sure that you don't remove "by kamaboko117" from any UI that you want to use instead. Also, be aware that if you change the prefab, future updates may break some stuff.
