using System;
using UnityEngine;


namespace TeleopReachy
{
    public class UserSize : Singleton<UserSize>
    {
        public float UserShoulderHeadDistance { get; private set; }
        //public float UserArmSize { get; set; }

        public float leftUserArmSize { get; set; }
        public float rightUserArmSize { get; set; }
        public float UserShoulderWidth { get; private set; }

        protected override void Init()
        {
            UserShoulderHeadDistance = 0.15f;
        }

        // public void UpdateUserSizeafterCalibration(double userArmSize, double userShoulderWidth)
        // {
        //     UserArmSize = (float)userArmSize;
        //     UserShoulderWidth = (float) userShoulderWidth;
        //     UpdateUserSizeEvent(EventArgs.Empty);
        // }

        public void UpdateUserSizeafterCalibration_differentarms(double LeftUserArmSize, double RightUserArmSize, double userShoulderWidth)
        {

            leftUserArmSize = (float)LeftUserArmSize;
            rightUserArmSize = (float)RightUserArmSize;
            UserShoulderWidth = (float)userShoulderWidth;
            Debug.Log("LeftUserArmSize: " + leftUserArmSize + " RightUserArmSize: " + rightUserArmSize + " UserShoulderWidth: " + UserShoulderWidth);
            UpdateUserSizeEvent(EventArgs.Empty);
        }


        public void UpdateUserSize(float userSize)
        {
            UserShoulderHeadDistance = userSize * 0.118f;
            leftUserArmSize = userSize * 0.336f;
            rightUserArmSize = userSize * 0.336f;
            //UserArmSize = userSize * 0.336f;
            UserShoulderWidth = userSize * 0.129f;
            UpdateUserSizeEvent(EventArgs.Empty);
        }

        void UpdateUserSizeEvent(EventArgs e)
        {
            EventHandler<EventArgs> handler = OnUpdateUserSize;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<EventArgs> OnUpdateUserSize;
    }
}
