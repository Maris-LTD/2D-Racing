namespace Game.Car.Data{
    using System;
    using System.Collections.Generic;
    using UnityEngine;


    [Serializable]
    public class CarData {
        public float maxSpeed;
        public float acceleration;
        public Sprite carSprite;
    }

    [CreateAssetMenu(fileName = "CarDataList", menuName = "Car/CarDataList")]
    public class CarDataList : ScriptableObject {
        public List<CarData> carDataList;
    }
}