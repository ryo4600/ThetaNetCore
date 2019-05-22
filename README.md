# ThetaNetCore
.NET Library for controlling Ricoh Theta Cameras

This library is build base on the API specification provided by Ricoh.
If you are not clear about the details, you may check their web page.
[API v2 Reference](https://developers.theta360.com/en/docs/v2.1/api_reference/)

The library is written with .Net Standard 2.0, so it should work for other OS using Xamarin.

## Architecture
*ThetaWifiApi* is almost direct translations of API specification. *ThetaWifiConnect* is a wrapper of API and you usually need this class for controlling the camera. Use *ThetaWifiApi*, if you need to do simple tasks or direct manipulation. In that case retrieve the instance of *ThetaWifiApi* from the instance of *ThetaWifiConnect*.

Wifi\Request and Wifi\Respond are self explanetory. They are just wrapping the data sent/retrieve from the camera.

## How to use
1. Declare an instance of ThetaWifiConnect.

    ```
    ThetaWifiConnect _theta = new ThetaWifiConnect();
    ```

1. You may register listeners 

	```
    _theta.ImageReady += ThetaApi_ImageReady;
	_theta.OnTakePictureCompleted += ThetaApi_OnTakePictureCompleted;
	_theta.OnTakePictureFailed += ThetaApi_OnTakePictureFailed;
    ```

1. This one checks if the connecion is ready
    ```
    await _theta.CheckConnection();
    ```
1. Get the status of the camera
    ```
	var status = await _theta.ThetaApi.StateAsync();
    ```
    'status' holds everything

1. When you start preview
    ```
	_theta.StartLivePreview();
	```
    ImageReady events are called many times. It's a Jpeg format.
    Beware that it is not an UI thread.
    ```
    private void ThetaApi_ImageReady(byte[] imgByteArray)
    {
        // JPEG format data
        this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
        {
            try
            {
                var source = LoadImage(imgByteArray);
                imgPreview.Source = source;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

        }));
    }
    ```
    Stop previewing. 
	```
	_theta.StopLivePreview();
    ```

1. Taking picture is also simple.
    ```
	await _theta.TakePictureAsync();
    ```
    When succeeded, completed event will be called.
    ```
    private void ThetaApi_OnTakePictureCompleted(string fileName)
    {
        ...
    }
    ```
    And if it failed, failed event will be called.
    ```
    private void ThetaApi_OnTakePictureFailed(Exception ex)
	{
        ...
    }
    ```

## About sample
ThetaNetSample is a WPF windows application. It shows you the basic usages of this library. Check the MainWindow.xaml and .xaml.cs, you will find everything you need.

If you want to show images in 360, you need to work yourself. I recommend to use the Unity and feed images into it.