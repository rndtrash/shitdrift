﻿using Sandbox;
using System;

namespace ShitDrift
{
	class SDCamera : Camera
	{
		[UserVar]
		public static bool sd_cam_collision { get; set; } = true;
		public float Elevation;

		Vector3 lastPos;
		float minFOV;
		float maxFOV;
		float maxSpeed;

		SDPlayer pawn;

		public SDCamera() : this( 250.0f, 70.0f, 90.0f, 350.0f )
		{
		}

		public SDCamera( float initialElevation, float minFOV, float maxFOV, float maxSpeed )
		{
			Elevation = initialElevation;
			this.minFOV = minFOV;
			this.maxFOV = maxFOV;
			this.maxSpeed = maxSpeed;

			FieldOfView = minFOV;
			Rot = Rotation.FromPitch(90.0f);
			Viewer = null;

			pawn = Local.Pawn as SDPlayer;
			if ( pawn != null )
				Pos = lastPos = pawn.EyePos;
		}

		public override void Update()
		{
			Pos = pawn.EyePos;
			var targetPos = Pos + Vector3.Up * Elevation;

			if ( sd_cam_collision )
			{
				var tr = Trace.Ray( Pos, targetPos )
					.Ignore( pawn )
					.Radius( 8 )
					.Run();

				Pos = tr.EndPos;
			}
			else
			{
				Pos = targetPos;
			}

			var speed = Pos.Distance( lastPos ) / Time.Delta;
			FieldOfView = Lerp(FieldOfView, MapFOV( speed ), 5f * Time.Delta);

			lastPos = Pos;
		}

		public override void BuildInput( InputBuilder input )
		{
			input.AnalogMove = input.AnalogMove.Normal;
			input.InputDirection = input.AnalogMove;
			input.ViewAngles = new Angles( 0, pawn.angleLocal, 0 );

			input.StopProcessing = true;
		}
		private float MapFOV( float value )
		{
			return Math.Min(value, maxSpeed) / maxSpeed * (maxFOV - minFOV) + minFOV;
		}

		private static float Lerp( float from, float to, float by)
		{
			if ( to > from )
				return to;
			return from + ((to - from) * by);
		}
	}
}
