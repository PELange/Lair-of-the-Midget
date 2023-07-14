![image](https://github.com/PELange/Lair-of-the-Midget/assets/63419814/f76c3d6e-d906-4052-8905-c067791f2846)

# Lair-of-the-Midget
Lair of the Midget - a top down dungeon fighter

Team up with your friends and fight you way through the endless dungon through progressively chellanging enemies.

![scene_damage_combined_v3](https://github.com/PELange/Lair-of-the-Midget/assets/63419814/70f9f35f-8af6-4b13-a8f9-1e66a2ccae3b)
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
