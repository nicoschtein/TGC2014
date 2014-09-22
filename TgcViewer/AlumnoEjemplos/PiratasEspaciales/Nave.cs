﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using TgcViewer;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.PiratasEspaciales
{
    public class Nave
    {
        public float VelocidadMovimiento { get; set; }
        public float Flotacion { get; set; }
        public float DireccionFlotacion { get; set; }
        public float VelocidadRotacion { get; set; }
        public float TiempoParado { get; set; }
        public TgcMesh Modelo { get; set; }
        public List<Disparo> Disparos { get; set; }
        public float TiempoRecarga { get; set; }

        public Nave()
        {
            Flotacion = 5f;
            VelocidadMovimiento = 200f;
            DireccionFlotacion = 1f;
            VelocidadRotacion = 30f;
            TiempoParado = 0F;
            TiempoRecarga = 1f;
            Disparos = new List<Disparo>();
        }

        public void Iniciar(TgcScene naves)
        {
            this.Modelo = naves.Meshes[0];

            this.Modelo.move(0,0,1500);
        }

        public void Movimiento(float tiempoRenderizado, List<TgcMesh> obstaculos)
        {
            bool rotando = false;
            bool seMovio = false;
            float mover = 0f;
            float rotar = 0f;
            TgcD3dInput input = GuiController.Instance.D3dInput;
            Vector3 movimiento = new Vector3(0, 0, 0);

            if (input.keyDown(Key.Left) || input.keyDown(Key.A))
            {
                rotando = true;
                rotar = -VelocidadRotacion;
            }
            else if (input.keyDown(Key.Right) || input.keyDown(Key.D))
            {
                rotando = true;
                rotar = VelocidadRotacion;
            }
            if (input.keyDown(Key.Up) || input.keyDown(Key.W))
            {
                seMovio = true;
                mover = -VelocidadMovimiento;
                Modelo.moveOrientedY(mover * tiempoRenderizado);
            }
            else if (input.keyDown(Key.Down) || input.keyDown(Key.S))
            {
                seMovio = true;
                mover = VelocidadMovimiento;
                Modelo.moveOrientedY(mover * tiempoRenderizado);
            }
            if ( input.keyDown(Key.R))
            {
                seMovio = true;
                mover = -VelocidadMovimiento;
                Modelo.move(0,mover*tiempoRenderizado,0);
            }
            else if (input.keyDown(Key.T))
            {
                seMovio = true;
                mover = VelocidadMovimiento;
                Modelo.move(0, mover * tiempoRenderizado, 0);
            }

            if (seMovio)
            {
                Vector3 ultimaPosicion = Modelo.Position;
                bool colisiono = false;
                foreach (TgcMesh obstaculo in obstaculos)
                {
                    TgcCollisionUtils.BoxBoxResult result = TgcCollisionUtils.classifyBoxBox(Modelo.BoundingBox, obstaculo.BoundingBox);
                    if (result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando)
                    {
                        colisiono = true;
                        break;
                    }
                }

                if (colisiono)
                {

                    Modelo.Position = ultimaPosicion;

                }                              
            }

            if (rotando)
            {
                Modelo.rotateY(Geometry.DegreeToRadian(rotar * tiempoRenderizado));
                GuiController.Instance.ThirdPersonCamera.rotateY(Geometry.DegreeToRadian(rotar * tiempoRenderizado));
;
            }

            
        }

        public void FlotacionEspacial(float tiempoRenderizado)
        {
            Modelo.move(0, Flotacion * DireccionFlotacion * tiempoRenderizado * 2, 0);
            if (FastMath.Abs(Modelo.Position.Y) > 7f)
            {
                DireccionFlotacion *= -1;
            }
        }

        public void Disparar(float tiempoRenderizado)
        {
            if (TiempoParado == 0 || TiempoParado >= TiempoRecarga)
            {
                
            
            TgcD3dInput input = GuiController.Instance.D3dInput;
            if (GuiController.Instance.D3dInput.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                Disparo disparo = new Disparo(Modelo);
                Disparos.Add(disparo);

            }
                TiempoParado = 0f;
            }
            TiempoParado = TiempoParado + tiempoRenderizado*4;
        }

        public void Renderizar(float tiempoRenderizado,List<TgcMesh> obstaculos)
        {

            this.Movimiento(tiempoRenderizado,obstaculos);
            this.Disparar(tiempoRenderizado);
            if (Disparos != null)
            {
                foreach (Disparo disparo in Disparos)
                {
                    
                    disparo.Actualizar(tiempoRenderizado, obstaculos);
                    if (disparo.TiempoDeVida - tiempoRenderizado <= 0)
                    {
                        disparo.EnJuego = false;
                        disparo.TestDisparo.dispose();
                    }
                }
                
                Disparos.RemoveAll(x => x.EnJuego == false);
            }
            //la flotacion requiere mejoras. Agustin S.
            //this.FlotacionEspacial(tiempoRenderizado);
            Modelo.render();
            Modelo.BoundingBox.render();
        }

    }
}