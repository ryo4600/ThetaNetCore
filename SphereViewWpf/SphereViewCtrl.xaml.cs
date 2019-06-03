using SphereViewWpf.Utils;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace SphereViewWpf
{
	/// <summary>
	/// SphereViewCtrl.xaml の相互作用ロジック
	/// </summary>
	public partial class SphereViewCtrl : UserControl, INotifyPropertyChanged
	{
		#region Members
		private MeshGeometry3D _sphereMesh = null;
		private ImageBrush _textureBrush = null;
		/// <summary>
		/// Horizontal orientation
		/// </summary>
		private double _angleTheta = 180;
		/// <summary>
		/// Vertical orientation
		/// </summary>
		private double _anglePhi = 90;
		#endregion

		/// <summary>
		/// Camera horizontal orientation
		/// </summary>
		public double Theta { get { return _angleTheta; } }

		/// <summary>
		/// Camera vertical orientation
		/// </summary>
		public double Phi { get { return _anglePhi; } }

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
		private void Notify(String propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		public SphereViewCtrl()
		{
			InitializeComponent();
			_sphereMesh = GeomTools.CreateSphereMesh(40, 20, 10); 

			// Initialize background with no image
			_textureBrush = new ImageBrush();
			_textureBrush.TileMode = TileMode.Tile;
		}


		/// <summary>
		/// Image source
		/// </summary>
		public ImageSource Source
		{
			get { return _textureBrush.ImageSource; }
			set
			{
				_visual3d.Children.Clear();
				_textureBrush.ImageSource = value;

				ModelVisual3D sphereModel = new ModelVisual3D();
				sphereModel.Content = new GeometryModel3D(_sphereMesh, new DiffuseMaterial(_textureBrush));
				_visual3d.Children.Add(sphereModel);
			}
		}

		Point? _dragStartPos = null;

		/// <summary>
		/// Mouse down : start dragging
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			this.Cursor = Cursors.SizeAll;
			_dragStartPos = e.GetPosition(_viewport);
			((FrameworkElement)sender).CaptureMouse();
		}

		/// <summary>
		/// Mouse up : end dragging
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnMouseUp(object sender, MouseButtonEventArgs e)
		{
			_dragStartPos = null;
			((FrameworkElement)sender).ReleaseMouseCapture();
			this.Cursor = Cursors.Arrow;
		}

		/// <summary>
		/// Mouse move : rotate view while dragging
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnMouseMove(object sender, MouseEventArgs e)
		{
			if (_dragStartPos == null)
				return;

			var currPos = e.GetPosition(_viewport);
			var deltaX = currPos.X - _dragStartPos.Value.X;
			var deltaY = currPos.Y - _dragStartPos.Value.Y;

			_angleTheta -= deltaX / this.ActualWidth * 180.0;
			_anglePhi -= deltaY / this.ActualHeight * 90.0;

			if (_angleTheta < 0)
				_angleTheta += 360;
			else if (_angleTheta > 360)
				_angleTheta -= 360;

			if (_anglePhi < 0.01)
				_anglePhi = 0.01;
			else if (_anglePhi > 179.99)
				_anglePhi = 179.99;

			_camera.LookDirection = GeomTools.CalcNormal( GeomTools.Deg2Rad(_angleTheta), GeomTools.Deg2Rad(_anglePhi));

			_dragStartPos = currPos;
		}

		/// <summary>
		/// Mouse wheel : zoom in / out the view
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnMouseWheel(object sender, MouseWheelEventArgs e)
		{
			_camera.FieldOfView -= e.Delta / 100;
			if (_camera.FieldOfView < 1) _camera.FieldOfView = 1;
			else if (_camera.FieldOfView > 140) _camera.FieldOfView = 140;

		}


	}
}
