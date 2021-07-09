using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameServer
{
    [RequireComponent(typeof(CharacterController))]
    public class WorldPlayer : MonoBehaviour
    {
        [SerializeField] float walkSpeed = 6.0f;
        [SerializeField] float runSpeed = 8.0f;
        [SerializeField] float jumpSpeed = 8.0f;
        [SerializeField] float gravity = 20.0f;

        CharacterController controller;

        public Player player { get; private set; }
        public Rigidbody rbody;

        Vector3 moveDirection = Vector3.zero;
        Vector3 previousPosition = Vector3.zero;
        byte previusAnimationSpeed = 0;
        InputBuffer inputBuffer = new InputBuffer();

        public void SetUp(Player player)
        {
            this.player = player;

            controller = GetComponent<CharacterController>();
            rbody = GetComponent<Rigidbody>();

            previousPosition = transform.position;
        }

        void Update()
        {
            Move();
            
            if (transform.position != previousPosition)
            {
                Server.getInstance.MovePlayer(player, transform.position, transform.rotation);
                previousPosition = transform.position;
            }
        }

        void Move()
        {
            _Input i = new _Input(0f, 0f, false);
            if (inputBuffer.hasInput)
                i = inputBuffer.Get();

            float _speed = walkSpeed;
            if (i.running)
                _speed = runSpeed;
            Debug.Log("i.running " + i.running);
            /*
            if (controller.isGrounded)
            {
                moveDirection = new Vector3(i.horizontal * _speed, 0.0f, i.vertical * _speed);
                moveDirection *= _speed;
            }


            if (!controller.isGrounded)
				moveDirection.y -= gravity * Time.deltaTime;*/
            moveDirection = new Vector3(i.horizontal * walkSpeed, 0.0f, i.vertical * walkSpeed);
            
            //moveDirection.y = -Physics.gravity.magnitude * Time.fixedDeltaTime;
            //Debug.Log("down to ground = " + -Physics.gravity.magnitude * Time.fixedDeltaTime);

            //rbody.AddForce(moveDirection * Time.fixedDeltaTime, ForceMode.VelocityChange);
            Debug.Log("move : " + moveDirection);
            //System.Threading.Thread.Sleep(500);

            //controller.Move(moveDirection * Time.deltaTime);
            rbody.MovePosition(transform.position + Time.deltaTime * walkSpeed * transform.TransformDirection(moveDirection.x, 0f, moveDirection.z));
            
            byte s = 0;
            if (i.horizontal != 0.0f || i.vertical != 0.0f)
            {
                if (i.running)
                    s = 2;
                else
                    s = 1;
            }
            if (previusAnimationSpeed != s)
            {
                Server.getInstance.AnimatePlayer(player, s);
                
                previusAnimationSpeed = s;
            }
            Debug.Log("anim : " + s.ToString());
        }
        public void Move(float horizontal, float vertical, bool running)
        {
            inputBuffer.Add(horizontal, vertical, running);
        }
        public void Jump()
        {
            float jump = 400;
            rbody.AddForce(jump * rbody.mass * Time.deltaTime * Vector3.up, ForceMode.Impulse);
            /*if (controller.isGrounded)
                moveDirection.y = jumpSpeed;*/

            //controller.Move(moveDirection * Time.deltaTime);
        }
    }
}