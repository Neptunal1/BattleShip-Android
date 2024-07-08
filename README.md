# BattleShip Game Android (Xamarin)



<img src ="https://github.com/Neptunal1/BattleShip/blob/main/Resources/drawable/howtoplay.png">



## Install

- Install [TASM](https://shreyasjejurkar.com/2017/03/27/how-to-install-and-configure-tasm-on-windows-7810/)
and put it under C:\

- Install [DOSBox](https://www.dosbox.com/download.php?main=1)
- Clone this repository to C:\tasm\bin\
- Open up DOSBox and run the game using this commands
```
 
Mount c: c:/
c:
cd tasm
cd bin
cycles=max
tasm /zi seanproj.asm
tlink /v seanproj.obj
seanproj
 
 ``` 
