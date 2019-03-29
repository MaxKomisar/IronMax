using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Iron_Max.Particle
{
    class GParticleSystem
    {
        public List<Particle> particles;
        private Texture2D dot; // текстура точки
        private Texture2D smoke; // текстура дыма



        private Random random;

        public GParticleSystem()
        {
            this.particles = new List<Particle>();
            random = new Random();
        }

        public void LoadContent(ContentManager Manager)
        {

            dot = Manager.Load<Texture2D>("particles/shell");
            smoke = Manager.Load<Texture2D>("particles/shell");


        }

        public void EngineRocket(Vector2 position) // функция, которая будет генерировать частицы
        {
            for (int a = 0; a < 20; a++) // создаем 2 частицы дыма для трейла
            {
                Vector2 velocity = new Vector2(0,10); //AngleToV2(5f, 0.6f);
                float angle = 0;
                float angleVel = 0;
                Vector4 color = new Vector4(1f, 1f, 1f, 1f);
                float size = 1f;
                int ttl = 190; // Вркмя и дальность полёта частицы
                float sizeVel = 0;
                float alphaVel = 0;


                GenerateNewParticle(smoke, position, velocity, angle, angleVel, color, size, ttl, sizeVel, alphaVel);
            }

            


        }

        private Particle GenerateNewParticle(Texture2D texture, Vector2 position, Vector2 velocity,
            float angle, float angularVelocity, Vector4 color, float size, int ttl, float sizeVel, float alphaVel)
        {
            Particle particle = new Particle(texture, position, velocity, angle, angularVelocity, color, size, ttl, sizeVel, alphaVel);
            particles.Add(particle);
            return particle;
        }

        public void Update(GameTime gameTime)
        {

            for (int particle = 0; particle < particles.Count; particle++)
            {

                particles[particle].Update();
                if (particles[particle].Size <= 0 || particles[particle].TTL <= 0) // если время жизни частички или её размеры равны нулю, удаляем её
                {


                    particles.RemoveAt(particle);
                    particle--;
                }
            }

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // ставим режим смешивания Addictive

            for (int index = 0; index < particles.Count; index++) // рисуем все частицы
            {
                particles[index].Draw(spriteBatch);
            }


        }

        public Vector2 AngleToV2(float angle, float length)
        {
            Vector2 direction = Vector2.Zero;
            direction.X = (float)Math.Cos(angle) * length;
            direction.Y = -(float)Math.Sin(angle) * length;
            return direction;
        }
    }
}
