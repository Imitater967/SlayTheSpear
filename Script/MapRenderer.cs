using System;
using UnityEngine;

namespace Script {
    public class MapRenderer : MonoBehaviour {
        public RoomView RoomPrefab;
        private MapGenerator Generator;
        public RoomView[][] RoomViews;
        private void Awake() {
            Generator = GetComponent<MapGenerator>();
            RoomViews = new RoomView[Generator.floors][];
        }

        private void Start() {
            for (var i = 0; i < Generator.mapData.Length; i++) {
                RoomViews[i] = new RoomView[Generator.mapWidth];
                
                for (var j = 0; j < Generator.mapData[i].Length; j++) {
                    var roomModel = Generator.mapData[i][j];
                    if (roomModel.type == Room.Type.NOT_ASSIGNED) {
                        continue;
                    }
                    var room = Instantiate(RoomPrefab, transform);
                    ((RectTransform)room.transform).anchoredPosition = roomModel.position;
                    RoomViews[i][j] = room;
                }
            }

            for (var i = 0; i < RoomViews.Length; i++) {
                for (var j = 0; j < RoomViews[i].Length; j++) {
                    var view = RoomViews[i][j];
                    if (Generator.mapData[i][j].nextRooms.Count == 0) {
                        continue;
                    }
                    foreach (var nextRoom in Generator.mapData[i][j].nextRooms) {
                        var parentView = RoomViews[nextRoom.row][nextRoom.column];
                        // if (parentView == null) {
                        //
                        //     Debug.Log($"行{nextRoom.row}列{nextRoom.column}没有View");
                        //     continue;
                        // }
                        view.SetRoomType(Generator.mapData[i][j].type);
                        view.SetNextPosition(((RectTransform)parentView.transform).anchoredPosition - (view.transform as RectTransform).anchoredPosition);

                    }
                }
            }
        }
    }
}