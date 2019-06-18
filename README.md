# witch-fantasy
Hi all, currently I'm in a slump because I'm stopping all work on this game <s>because it sucks</s>, so it'll take a while for me to regain
momentum again. I'll add more stuff to the README when I figure out something meaningful to say. 

When you're ready to contribute, check out the Project menu for the long term roadmap on what to do. I'll open up issues with reference 
to the Project tab sometime. If you want to be a collaborator, please DM me your GitHub ID on Discord. Note that this option is only open to those whom I know personally on Discord.

In the meantime, here's how to open the game. 
1. Download Unity: https://unity3d.com/get-unity/download.  
   Install Unity Hub, then use that to install the latest version of Unity
2. Download the project into a folder and open the folder itself using Unity.  
   If you encounter a warning that says that the Unity version used by the project differs from your Unity version, don't worry. Just 
   let it update all the files. We will decide on a standardised version of Unity to use later on.
3. After opening the project, setup the multiplayer settings by going to Window > Photon Unity Networking > Highlight Server Settings.  
   The "Window" button is on the top ribbon.
4. Under PhotonServerSettings, check "Use Name Server", then enter "ce449a90-b0a9-4684-9f88-db63d282d27d" as the App Id Realtime. Also 
   clear the IP address under Server and leave the port as 5055.  
   Eventually I'll add an offline mode so that anyone can test. Later down the road, I'll find a way to host the game through localhost, 
   and then using a dedicated server. 
5. Ensure that the scene you're looking at is "Test Lobby". Press the Play button and you're good to go!  

Oh yeah, when you're midair, use W and S to pitch, Q and E to yaw, A and D to roll, Space to move forward. 
When you're on ground, use WASD to move around, Space to jump. Press Space in midair to switch to flying mode. 
To fire, hold Right Click, then while holding, Left Click to fire. The bullet regenerates every 3 seconds. 

Want to preview the game? See https://danielnyan.github.io/witch-fantasy/index.html

Some notes: 
The source files for the WebGL build meant to showcase the basic concepts of the game. This game is unfinished and it will probably remain so. 

Also note that the colour space is converted to gamma space while the game originally uses linear colour space, so colours shown will differ from the final product.

Finally, I haven't tested multiplayer yet, but the behaviour coded in is that if the host player disconnects, everyone disconnects. I wanted to set up some sort of server authority kind of thing where the host calculates all the information, and I haven't figured out how to hand down all this info to the next host when the main guy leaves.

Copyright 2019, danielnyan
