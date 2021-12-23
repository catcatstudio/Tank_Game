using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace Complete
{
    public class TankShooting : MonoBehaviourPunCallbacks
    {
        public int m_PlayerNumber = 1;              // Used to identify the different players.
        public Rigidbody m_Shell;                   // Prefab of the shell.
        public Transform m_FireTransform;           // A child of the tank where the shells are spawned.
        public Slider m_AimSlider;                  // A child of the tank that displays the current launch force.
        public AudioSource m_ShootingAudio;         // Reference to the audio source used to play the shooting audio. NB: different to the movement audio source.
        public AudioClip m_ChargingClip;            // Audio that plays when each shot is charging up.
        public AudioClip m_FireClip;                // Audio that plays when each shot is fired.
        public float m_MinLaunchForce = 15f;        // The force given to the shell if the fire button is not held.
        public float m_MaxLaunchForce = 30f;        // The force given to the shell if the fire button is held for the max charge time.
        public float m_MaxChargeTime = 0.75f;       // How long the shell can charge for before it is fired at max force.


        private string m_FireButton;                // The input axis that is used for launching shells.
        private float m_CurrentLaunchForce;         // The force that will be given to the shell when the fire button is released.
        private float m_ChargeSpeed;                // How fast the launch force increases, based on the max charge time.
        private bool m_Fired;                       // Whether or not the shell has been launched with this button press.


        public override void OnEnable()
        {
            base.OnEnable();
            // When the tank is turned on, reset the launch force and the UI
            m_CurrentLaunchForce = m_MinLaunchForce;
            m_AimSlider.value = m_MinLaunchForce;
        }


        private void Start ()
        {
            // The fire axis is based on the player number.
            m_FireButton = "Fire" + m_PlayerNumber;

            // The rate that the launch force charges up is the range of possible forces by the max charge time.
            m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
        }


        private void Update ()
        {
            //飛彈最小默認值
            m_AimSlider.value = m_MinLaunchForce;

            //如果已經超過最大力並且砲彈還沒有發射......
            if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
            {
                // 使用最大力量 並且發射
                m_CurrentLaunchForce = m_MaxLaunchForce;
                Fire ();
            }
            //如果按下按鈕的話.......
            else if (Input.GetButtonDown (m_FireButton))
            {
                //重置發射標誌並重置發射力
                m_Fired = false;
                m_CurrentLaunchForce = m_MinLaunchForce;

                //將clip更改為ChargingClip並且播放
                m_ShootingAudio.clip = m_ChargingClip;
                m_ShootingAudio.Play ();
            }
            //如果按住開火按鈕而外殼尚未啟動......
            else if (Input.GetButton (m_FireButton) && !m_Fired)
            {
                //增加發射力並更新滑塊
                m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

                m_AimSlider.value = m_CurrentLaunchForce;
            }
            //如果釋放開火按鈕並且外殼尚未啟動......
            else if (Input.GetButtonUp (m_FireButton) && !m_Fired)
            {
                //開火
                Fire ();
            }
        }


        private void Fire()
        {
            //飛彈飛射
            m_Fired = true;
            //創建新的飛彈 且套用鋼體
            Rigidbody shellInstance = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;
            //傳"FirOther"函式給 RpcTarget.Others   => 傳送m_FireTransform.position
            photonView.RPC("FireOther", RpcTarget.Others, m_FireTransform.position);
            //飛彈的力量*方向
            shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward;
            //聲音
            m_ShootingAudio.clip = m_FireClip;
            //撥放
            m_ShootingAudio.Play();
            //重置發射力。 這是在缺少按鈕事件的情況下的預防措施。
            m_CurrentLaunchForce = m_MinLaunchForce;
        }
        //傳送資料
        [PunRPC]
        private void FireOther(Vector3 pos) //設定FireOther函式
        {
            //pos是Tank的位置

            //飛彈飛射
            m_Fired = true;
            //產生新的飛彈
            Rigidbody shellInstance = Instantiate(m_Shell, pos, m_FireTransform.rotation) as Rigidbody;
            //飛彈的力量*方向
            shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward;
            //重置發射力。 這是在缺少按鈕事件的情況下的預防措施。
            m_CurrentLaunchForce = m_MinLaunchForce;

            //Debug.Log("位置是:"+pos);
        }         
        
    }
}