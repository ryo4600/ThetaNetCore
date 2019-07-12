using SphereViewWpf.Utils;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

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
		#endregion

		/// <summary>
		/// Camera horizontal orientation
		/// </summary>
		public double Theta { get; private set; } = 180;

		/// <summary>
		/// Camera vertical orientation
		/// </summary>
		public double Phi { get; private set; } = 90;

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

		Point? _lastDragPos = null;
		DispatcherTimer _mousePosCheckTimer = null;
		TimeSpan _tickInterval = new TimeSpan(0, 0, 0, 0, 100);
		DateTime _lastRecordedTime = DateTime.Now;
		Point? _lastRecordedPos = null;

		//int counter = 0;
		/// <summary>
		/// Mouse down : start dragging
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (_mousePosCheckTimer == null)
			{
				_mousePosCheckTimer = new DispatcherTimer();
				_mousePosCheckTimer.Interval = _tickInterval;
				_mousePosCheckTimer.Tick += (sender2, e2) =>
				{
					_lastRecordedPos = Mouse.GetPosition(_viewport);
					_lastRecordedTime = DateTime.Now;
				};
			}

			this.Cursor = Cursors.SizeAll;
			_lastRecordedPos = _lastDragPos = e.GetPosition(_viewport);
			_lastRecordedTime = DateTime.Now;

			((FrameworkElement)sender).CaptureMouse();

			_mousePosCheckTimer.Start();
		}

		/// <summary>
		/// Mouse move : rotate view while dragging
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnMouseMove(object sender, MouseEventArgs e)
		{
			if (_lastDragPos == null)
				return;

			var currPos = e.GetPosition(_viewport);
			//_lastDragTime = DateTime.Now;

			var now = DateTime.Now;

			var deltaX = currPos.X - _lastDragPos.Value.X;
			var deltaY = currPos.Y - _lastDragPos.Value.Y;

			DoRotate(deltaX, deltaY);
			_lastDragPos = currPos;

		}

		/// <summary>
		/// Mouse up : end dragging
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		async private void OnMouseUp(object sender, MouseButtonEventArgs e)
		{
			_mousePosCheckTimer?.Stop();
			((FrameworkElement)sender).ReleaseMouseCapture();

			if (_lastRecordedPos == null)
				return;

			var timeDiff = DateTime.Now - _lastRecordedTime;

			var currPos = e.GetPosition(_viewport);
			var inertiaX = (currPos.X - _lastRecordedPos.Value.X) / timeDiff.Milliseconds * 50;
			var inertiaY = (currPos.Y - _lastRecordedPos.Value.Y) / timeDiff.Milliseconds * 50;

			_lastDragPos = null;
			this.Cursor = Cursors.Arrow;

			// add inertia effect
			while (Math.Abs(inertiaX) > 0.1 && Math.Abs(inertiaY) > 0.1)
			{
				inertiaX *= 0.9;
				inertiaY *= 0.9;
				DoRotate(inertiaX, inertiaY);
				await Task.Delay(1);
			}
		}

		/// <summary>
		/// Rotate the view
		/// </summary>
		/// <param name="deltaX"></param>
		/// <param name="deltaY"></param>
		private void DoRotate(double deltaX, double deltaY)
		{
			Theta -= deltaX / this.ActualWidth * 180.0;
			Phi -= deltaY / this.ActualHeight * 90.0;

			if (Theta < 0)
				Theta += 360;
			else if (Theta > 360)
				Theta -= 360;

			if (Phi < 0.01)
				Phi = 0.01;
			else if (Phi > 179.99)
				Phi = 179.99;

			_camera.LookDirection = GeomTools.CalcNormal(GeomTools.Deg2Rad(Theta), GeomTools.Deg2Rad(Phi));
		}

		/// <summary>
		/// Mouse wheel : zoom in / out the view
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnMouseWheel(object sender, MouseWheelEventArgs e)
		{
			_camera.FieldOfView -= e.Delta / 100;
			if (_camera.FieldOfView < 1)
				_camera.FieldOfView = 1;
			else if (_camera.FieldOfView > 140)
				_camera.FieldOfView = 140;

		}

	}
}
