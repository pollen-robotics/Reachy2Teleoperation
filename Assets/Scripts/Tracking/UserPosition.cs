using System;
using UnityEngine;

namespace TeleopReachy
{
    public class UserPosition : Singleton<UserPosition>
    {
        [SerializeField]
        private Transform choosePositionMessage;

        public enum UserPositionEnum
        {
            Seated, SitStand, Standing
        }
        
        public UserPositionEnum Position { get; private set; }

        public void SeatedPosition()
        {
            Position = UserPositionEnum.Seated;
            choosePositionMessage.gameObject.SetActive(false);
        }

        public void SitStandPosition()
        {
            Position = UserPositionEnum.SitStand;
            choosePositionMessage.gameObject.SetActive(false);
        }

        public void StandingPosition()
        {
            Position = UserPositionEnum.Standing;
            choosePositionMessage.gameObject.SetActive(false);
        }
    }
}
