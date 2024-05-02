using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Script {
    public class MapGenerator : MonoBehaviour {
        /// <summary>
        /// X轴距离
        /// </summary>
        public float xDistance = 30;
        /// <summary>
        /// Y轴距离
        /// </summary>
        public float yDistance = 25;
        /// <summary>
        /// 房间随机移动量
        /// </summary>
        public float placementRandomness = 5;
        /// <summary>
        /// 垂直层数
        /// </summary>
        public int floors = 15;
        /// <summary>
        /// 水平层数
        /// </summary>
        public int mapWidth = 7;
        /// <summary>
        /// 需要生成的路径数量(初始节点数量)
        /// </summary>
        public int paths = 6;

        private Dictionary<Room.Type, float> randomRoomWeight = new Dictionary<Room.Type, float>();
        private float randomTotalWeight;

        //房间生成权重
        public float monsterRoomWeight = 10;
        public float shopRoomWeight = 2.5f;
        public float campfireRoomWeight = 4;

        public Room[][] mapData;

        private void Awake() {
            GenerateMap();
        }

        void GenerateMap() {
            mapData = GenerateInitinalGrid();
            var startingPoints = GetRandomStartingPoints();
            for (var j = 0; j < startingPoints.Length; j++) {
                var currentJ = startingPoints[j];
                for (int i = 0; i < floors - 1; i++) {
                    currentJ = setupConnection(i, currentJ);
                }
            }
            
            setupBossRoom();
            setupRandomRoomWeights();
            setupRoomTypes();

            // int k = 0;
            // foreach (var rooms in mapData) {
            //     Debug.Log($"floor {k}");
            //     var msg = rooms.Where(x => x.nextRooms.Count > 0).Select(x=>x.ToString());
            //     k += 1;
            //     StringBuilder sb = new StringBuilder();
            //     foreach (var s in msg) {
            //         sb.Append(s);
            //     }
            //     Debug.Log(sb);
            // }
        }

        void setupBossRoom() {
            var middle = Mathf.FloorToInt(mapWidth * 0.5f);
            var bossRoom = mapData[floors - 1][middle];
            for (int j = 0; j < mapWidth; j++) {
                var currentRoom = mapData[floors - 2][j];
                if (currentRoom.nextRooms.Count > 0) {
                    currentRoom.nextRooms.Clear();
                    currentRoom.nextRooms.Add(bossRoom);
                }
            }

            bossRoom.type = Room.Type.BOSS;
        }

        void setupRandomRoomWeights() {
            this.randomRoomWeight[Room.Type.MONSTER] = monsterRoomWeight;
            this.randomRoomWeight[Room.Type.CAMPFIRE] = monsterRoomWeight + campfireRoomWeight;
            this.randomRoomWeight[Room.Type.SHOP] = monsterRoomWeight + campfireRoomWeight + shopRoomWeight;
            randomTotalWeight = randomRoomWeight[Room.Type.SHOP];
        }

        void setupRoomTypes() {
            //第一层都是怪物
            foreach (var room in mapData[0]) {
                if (room.nextRooms.Count > 0) {
                    room.type = Room.Type.MONSTER;
                }
            }
            //第九层都是宝藏
            foreach (var room in mapData[floors/2]) {
                if (room.nextRooms.Count > 0) {
                    room.type = Room.Type.TREASURE;
                }
            }
            foreach (var room in mapData[floors-2]) {
                if (room.nextRooms.Count > 0) {
                    room.type = Room.Type.CAMPFIRE;
                }
            }
            
            foreach (var rooms in mapData) {
                foreach (var room in rooms) {
                    foreach (var roomNextRoom in room.nextRooms) {
                        if (roomNextRoom.type == Room.Type.NOT_ASSIGNED) {
                            setRoomRandomly(roomNextRoom);
                        }
                    }
                }
            }
        }

        void setRoomRandomly(Room roomToSet) {
            bool campfireBelow4 = true;
            bool consecutiveCampfire = true;
            bool consecutiveShop = true;
            bool campfireOn13 = true;
            Room.Type candidate = Room.Type.NOT_ASSIGNED;
            while (campfireBelow4 || consecutiveCampfire || consecutiveShop || campfireOn13) {
                candidate = getRandomRoomTypeByWeight();
                bool isCampfire = candidate == Room.Type.CAMPFIRE;
                bool hasCampfireParent = roomHasParentOfType(roomToSet, Room.Type.CAMPFIRE);
                bool isShop = candidate == Room.Type.SHOP;
                bool hasShopParent = roomHasParentOfType(roomToSet, Room.Type.SHOP);

                campfireBelow4 = isCampfire && roomToSet.row < 4;
                consecutiveCampfire = isCampfire && hasCampfireParent;
                consecutiveShop = isShop && hasShopParent;
                campfireOn13 = isCampfire && roomToSet.row == floors-2;
            }

            roomToSet.type = candidate;

        }

        Room.Type getRandomRoomTypeByWeight() {
            var roll = Random.Range(0, randomTotalWeight);
            foreach (var type in randomRoomWeight.Keys) {
                if (randomRoomWeight[type] > roll) {
                    return type;
                }
            }
            return Room.Type.MONSTER;
        }

        bool roomHasParentOfType(Room room, Room.Type type) {
            List<Room> parents = new List<Room>();
            //左节点
            if (room.column > 0 && room.row > 0) {
                var parent = mapData[room.row - 1][room.column - 1];
                if (parent.nextRooms.Contains(room)) {
                    parents.Add(parent);
                }
            }
            //中节点
            if (room.row > 0) {
                var parent = mapData[room.row - 1][room.column];
                if (parent.nextRooms.Contains(room)) {
                    parents.Add(parent);
                }
            }
            //右节点
            if (room.column < mapWidth-1 && room.row > 0) {
                var parent = mapData[room.row - 1][room.column + 1];
                if (parent.nextRooms.Contains(room)) {
                    parents.Add(parent);
                }
            }
            foreach (var parent in parents) {
                if (parent.type ==type) {
                    return true;
                }
            }

            return false;
        }

        private int setupConnection(int i, int j) {
            Room nextRoom = null;
            Room currentRoom = mapData[i][j];
            while (nextRoom==null || wouldCrossExisting(i,j,nextRoom)) {
                var randomJ = Math.Clamp(Random.Range(j - 1, j + 1 +1),0, mapWidth - 1);
                nextRoom = mapData[i + 1][randomJ];
            }
            currentRoom.nextRooms.Add(nextRoom);


            return nextRoom.column;
        }

        bool wouldCrossExisting(int i, int j, Room room) {
            Room leftNeighbour = null;
            Room rightNeighbour = null;
            //如果j==0,那么就没有左临节点
            if (j > 0) {
                leftNeighbour = mapData[i][j - 1];
            }
            //如果j>map width -1,那么就没有右临节点
            if (j < mapWidth - 1) {
                rightNeighbour = mapData[i][j + 1];
            }
            //右邻节点不能往左
            if (rightNeighbour != null && room.column > j) {
                foreach (var rightNeighbourNextRoom in rightNeighbour.nextRooms) {
                    //往左返回true
                    if (rightNeighbourNextRoom.column<room.column) {
                        return true;
                    }
                }
            }
            //左邻节点不能往右
            if (leftNeighbour != null && room.column < j) {
                foreach (var leftNeighbourNextRoom in leftNeighbour.nextRooms) {
                    //往左返回true
                    if (leftNeighbourNextRoom.column > room.column) {
                        return true;
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// 随机生成六个路径,并且确保两个点是不一样的
        /// </summary>
        /// <returns></returns>
        private int[] GetRandomStartingPoints() {
            List<int> yCoordinates = new List<int>();
            int uniquePoints = 0;
            while (uniquePoints<2) {
                uniquePoints = 0;
                yCoordinates.Clear();
                for (int i = 0; i < paths; i++) {
                    var startPoint = Random.Range(0, mapWidth - 1);
                    if (yCoordinates.Contains(startPoint)) {
                        uniquePoints += 1;
                    }
                    yCoordinates.Add(startPoint);
                }
            }

            return yCoordinates.ToArray();
        }
        

        /// <summary>
        /// 生成初始Grid
        /// </summary>
        /// <returns></returns>
        private Room[][] GenerateInitinalGrid() {
            var result = new Room[floors][];
            for (int i = 0; i < floors; i++) {
                var room = new Room[mapWidth];
                for (int j = 0; j < mapWidth; j++) {
                    //生成房间
                    var currentRoom = new Room();
                    //生成房间偏移量
                    var offset = Random.insideUnitCircle * placementRandomness;
                    currentRoom.row = i;
                    currentRoom.column = j;
                    currentRoom.position = new Vector2(j * xDistance, i * -yDistance) + offset;
                    
                    //boss房间的Y是非随机的
                    //用i+1为的是让boss有更大的空间
                    if (i == floors - 1) {
                        currentRoom.position.y = (i + 1) * -yDistance;
                    }
                    room[j] = currentRoom;
                }
                result[i] = room;
            }

            return result;
        }
    }
}