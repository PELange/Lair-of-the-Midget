![title](https://github.com/PELange/Lair-of-the-Midget/assets/63419814/d5f31549-0820-43b8-a20c-6799d4ffc36c)

# Lair-of-the-Midget
Lair of the Midget - a top down dungeon fighter

Team up with your friends and fight you way through the endless dungon through progressively chellanging enemies.

![scene_damage_combined_v4](https://github.com/PELange/Lair-of-the-Midget/assets/63419814/ef1cfa84-b395-4fd3-9152-17f74b270009)
1. Different enemies
2. Player
3. Minor and major health potions
4. Pillars in the dungeons to get distance from the enemies and take cover
5. Room nuber to keep track of the progress
6. The door to the next room unlocks when all the enemies are killed
7. Dead players leave behind a skull

Developed as part of the master studies at the HTW Berlin.
Graphics based on `0x72_DungeonTilesetII_v1.3.1`.

### Technical features:
- Written from scratch in C# (no game engines used)
- Procedual infinite dungeon generation
- Resilient networking with authoritive dedicated servers
- Custom batch sprite rendering pipeline built on top of OpenGL
- Custom physics implementation (Quadree + AABB-colliders + sweeping detection)
- AI behavior and pathfinding (with A* algorithm)
