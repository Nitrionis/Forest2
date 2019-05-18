# Forest
## Statistics representation
```C++
int trajectory records count;
Vector3[trajectory records count] positions;
Quaternion[trajectory records count] rotations;
int planes records count;
EnemyMoveInfo[planes records count] planesMovement;
int zombies records count;
EnemyMoveInfo[zombies records count] zombiesMovement;
int cars records count;
EnemyMoveInfo[cars records count] carsMovement;
int collision records count;
СollisionInfo[collision records count] collisionRecords;


struct EnemyMoveInfo
{
    public Vector3 startPos, endPos;
    public float startTime, endTime;
}

struct СollisionInfo
{
    public Vector3 pos;
    public float time;
}
```
