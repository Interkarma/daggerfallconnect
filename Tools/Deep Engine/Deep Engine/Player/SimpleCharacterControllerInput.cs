//
// Adapted from BEPUphysics Demos
// http://bepuphysics.codeplex.com/
//

#region Using Statements
using BEPUphysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace DeepEngine.Player
{
    /// <summary>
    /// Handles input and movement of a character in the game.
    /// Acts as a simple 'front end' for the bookkeeping and math of the character controller.
    /// </summary>
    public class SimpleCharacterControllerInput
    {
        /// <summary>
        /// Camera to use for input.
        /// </summary>
        public Camera Camera;

        /// <summary>
        /// Current offset from the position of the character to the 'eyes.'
        /// </summary>
        public Vector3 CameraOffset = new Vector3(0, .7f, 0);

        /// <summary>
        /// Physics representation of the character.
        /// </summary>
        public SimpleCharacterController CharacterController;

        /// <summary>
        /// Whether or not to use the character controller's input.
        /// </summary>
        public bool IsActive = true;

        /// <summary>
        /// Owning space of the character.
        /// </summary>
        public Space Space;


        /// <summary>
        /// Constructs the character and internal physics character controller.
        /// </summary>
        /// <param name="owningSpace">Space to add the character to.</param>
        /// <param name="CameraToUse">Camera to attach to the character.</param>
        public SimpleCharacterControllerInput(Space owningSpace, Camera CameraToUse)
        {
            CharacterController = new SimpleCharacterController(Vector3.Zero, 70, 64, 40f, 20);

            Space = owningSpace;
            Space.Add(CharacterController);


            Camera = CameraToUse;
            Deactivate();
        }

        /// <summary>
        /// Gives the character control over the Camera and movement input.
        /// </summary>
        public void Activate()
        {
            if (!IsActive)
            {
                IsActive = true;
                //Camera.UseMovementControls = false;
                CharacterController.Activate();
                CharacterController.Body.Position = (Camera.Position);
            }
        }

        /// <summary>
        /// Returns input control to the Camera.
        /// </summary>
        public void Deactivate()
        {
            if (IsActive)
            {
                IsActive = false;
                //Camera.UseMovementControls = true;
                CharacterController.Deactivate();
            }
        }


        /// <summary>
        /// Handles the input and movement of the character.
        /// </summary>
        /// <param name="dt">Time since last frame in simulation seconds.</param>
        /// <param name="previousKeyboardInput">The last frame's keyboard state.</param>
        /// <param name="keyboardInput">The current frame's keyboard state.</param>
        /// <param name="previousGamePadInput">The last frame's gamepad state.</param>
        /// <param name="gamePadInput">The current frame's keyboard state.</param>
        public void Update(float dt, KeyboardState previousKeyboardInput, KeyboardState keyboardInput, GamePadState previousGamePadInput, GamePadState gamePadInput)
        {
            if (IsActive)
            {
                //Note that the character controller's update method is not called here; this is because it is handled within its owning space.
                //This method's job is simply to tell the character to move around based on the Camera and input.

                //Puts the Camera at eye level.
                Camera.Position = CharacterController.Body.BufferedStates.InterpolatedStates.Position + CameraOffset;
                Vector2 totalMovement = Vector2.Zero;
#if !WINDOWS
                Vector3 forward = Camera.WorldMatrix.Forward;
                forward.Y = 0;
                forward.Normalize();
                Vector3 right = Camera.WorldMatrix.Right;
                totalMovement += gamePadInput.ThumbSticks.Left.Y * new Vector2(forward.X, forward.Z);
                totalMovement += gamePadInput.ThumbSticks.Left.X * new Vector2(right.X, right.Z);
                CharacterController.MovementDirection = Vector2.Normalize(totalMovement);

                //Jumping
                if (previousGamePadInput.IsButtonUp(Buttons.LeftStick) && gamePadInput.IsButtonDown(Buttons.LeftStick))
                {
                    CharacterController.Jump();
                }
#else


                //Collect the movement impulses.

                Vector3 movementDir;

                if (keyboardInput.IsKeyDown(Keys.W))
                {
                    movementDir = Camera.WorldA.Forward;
                    totalMovement += Vector2.Normalize(new Vector2(movementDir.X, movementDir.Z));
                }
                if (keyboardInput.IsKeyDown(Keys.S))
                {
                    movementDir = Camera.WorldA.Forward;
                    totalMovement -= Vector2.Normalize(new Vector2(movementDir.X, movementDir.Z));
                }
                if (keyboardInput.IsKeyDown(Keys.A))
                {
                    movementDir = Camera.WorldA.Left;
                    totalMovement += Vector2.Normalize(new Vector2(movementDir.X, movementDir.Z));
                }
                if (keyboardInput.IsKeyDown(Keys.D))
                {
                    movementDir = Camera.WorldA.Right;
                    totalMovement += Vector2.Normalize(new Vector2(movementDir.X, movementDir.Z));
                }
                if (totalMovement == Vector2.Zero)
                    CharacterController.MovementDirection = Vector2.Zero;
                else
                    CharacterController.MovementDirection = Vector2.Normalize(totalMovement);

                //Jumping
                if (previousKeyboardInput.IsKeyUp(Keys.Space) && keyboardInput.IsKeyDown(Keys.Space))
                {
                    CharacterController.Jump();
                }


#endif
            }
        }
    }
}