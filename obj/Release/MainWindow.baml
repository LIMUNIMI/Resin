   M S B A M L   `   `   `  ���� 	  Resin    Resin.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    >d2http://schemas.microsoft.com/expression/blend/2008    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     -     � Title$  Resin��$
��1440��$	��810�� � WindowStyle$
 None=� � 
ResizeMode$ NoResize=� � WindowStartupLocation$ CenterScreen=� � WindowState$ 	Maximized=�.���  � ColumnDefinitions �� $��9* ��� $	��11* �����  �� ImageSource$&  /Images/Backgrounds/DarkWood.png�� j� TileMode$
 Tile=� � RowDefinitions �� $��6* ��� $	��78* ��� $	��16* �.+��� -   	 ��Name$	 btnInit��$
��Init��$��	0,3,0,3.4q�$
��Left=�$��Center=�$��40��$��70��
�������$��2q�
�������$��Magneto)�$
��Bold"�$��19(�
��� d ��� $��0��$�� ��$��Center=�$��Center=�$��Magneto)�$��30(�
�������$
��Auto��$
��Auto��$��1��$��28,0,676,0.4q��� -   $	 btnExit��$��X��$��	0,3,5,3.4q�$��Right=�$��Center=�$��40��$��40��
�������$��2q�
�������$��Magneto)�$
��Bold"�$��19(�
���  ��$��1���� -   $	 btnTest��$
��Left=�$	��Top=�$��0��$
��Test��$��30��$��90��
�������$��2q�
�������$��Magneto)�$��16(�$��	80,10,0,0q����� $$ Images/Backgrounds/MidWood.png��$
 Tile=�
 j� Stretch$

 None=�� $��2��$��
0,46,0,0.2q�$��2�� �� $
��274* ��� $��1624* �.+��� $��0��$��2q�
�������$��2,2,2.4,2.4q����� $&  Images/Backgrounds/LightWood.jpg��$
 Tile=�._���  �� Orientation$ Vertical=�.+��� $��Audio Input��
���   �$��15(�$
��Bold"�$��SegoeUI)�$��2,2,2,2q��� -    ��Name$ lstWaveInSoundCards��$��22��$��2,2,2,2q��� $��Audio Output��
���   �$��15(�$
��Bold"�$��SegoeUI)�$��2,2,2,2q��� -   $ lstWaveOutSoundCards��$��22��$��2,2,2,2q��� $��MIDI Output��
���   �$��15(�$
��Bold"�$��SegoeUI)�$��2,2,2,2q��� $ 
Horizontal=�.+��� -   $	 btnMidiPortMinus��$��-��$��30��$��30��
�������$��2q�
�������$��Magneto)�$��19(�$��2,2,2,2q����� $$ Images/Backgrounds/MidWood.png��$
 Tile=�$

 None=��� -    ��Name$ lblMidiPort��$��-��$��30��$��30��
�������$��Digital-7 Italic)�$��18(�$��Center=�$��Center=�
���   �
�������$��2q��� -	   $	 btnMidiPortPlus��$��+��$��30��$��30��
�������$��2q�
�������$��Magneto)�$��19(�$��2,2,2,2q����� $$ Images/Backgrounds/MidWood.png��$
 Tile=�$

 None=��� $��FFT info��
���   �$��15(�$
��Bold"�$��SegoeUI)�$��2,2,2,2q��� 
���   �
�������$��2q�$��2,2,2,2q�._��  �� $
��Auto ��� $
��Auto � �� $��* ��� $��* ��� $��* ��� $��* �.+��� $��0��$��0��$��	Peak X = ��
��������� -
   $��lblPeakX��$��1��$��0��$��-��
��������� $��0��$��1��$��Peak X (smth) = ��
��������� -   $��lblPeakXsmooth��$��1��$��1��$��-��
��������� $��0��$��2��$��	Peak Y = ��
��������� -   $��lblPeakY��$��1��$��2��$��-��
��������� $��0��$��3��$��Note = ��
��������� -   $��lblNote��$��1��$��3��$��-��
��������� $��Head Tracker��
���   �$��15(�$
��Bold"�$��SegoeUI)�$��2,2,2,2q��� $ 
Horizontal=�.+��� -   $	 btnCalibrateHeadTracker��$��	Calibrate��$��30��$��90��
�������$��2q�
�������$��Magneto)�$��16(�$��2,2,2,2q����� $$ Images/Backgrounds/MidWood.png��$
 Tile=�$

 None=��� -   $	 
btnHTportM��$��<��$��30��$��30��
�������$��2q�
�������$��Magneto)�$��19(�$��2,2,2,2q����� $$ Images/Backgrounds/MidWood.png��$
 Tile=�$

 None=��� -   $ 	lblHTport��$��-��$��30��$��30��
�������$��Digital-7 Italic)�$��18(�$��Center=�$��Center=�
���   �
�������$��2q��� -   $	 
btnHTportP��$��>��$��30��$��30��
�������$��2q�
�������$��Magneto)�$��19(�$��2,2,2,2q����� $$ Images/Backgrounds/MidWood.png��$
 Tile=�$

 None=��� 
���   �
�������$��2q�$��2,2,2,2q�._��  �� $
��Auto ��� $
��Auto � �� $��* ��� $��* ��� $��* ��� $��* �.+��� $��0��$��0��$��Pitch = ��
��������� -   $��
lblHTpitch��$��1��$��0��$��-��
��������� $��0��$��1��$��Yaw = ��
��������� -   $��lblHTyaw��$��1��$��1��$��-��
��������� $��0��$��2��$��Roll = ��
��������� -   $��	lblHTroll��$��1��$��2��$��-��
��������� $��0��$��3��$��Speed = ��
��������� -   $��
lblHTspeed��$��1��$��3��$��-��
��������� $��Switches��
���   �$��15(�$
��Bold"�$��SegoeUI)�$��2,2,2,2q�� $��2,2,2,2q� �� $��* ��� $��* �.+��� $��0��$ Vertical=�.+��� -    ��Name$ cbxPlaySines��$��
Play sines��$��20��$
��Bold"��� -   $ 
cbxAudioIn��$��Audio in��$��20��$
��Bold"��� -   $ cbxPlot��$
��Plot��$��20��$
��Bold"��� -   $ cbxHeadtracker��$��	HeadTrack��$��20��$
��Bold"��� $��1��$ Vertical=�.+��� -    ��Name$ sldAudioOutVolume��
���   �$��Stretch=�$��20�� � Minimum$ 0Q� � Maximum$	 100Q� �� Interval$ 1���� $ 
Horizontal=�.+��� $��L��$��20��$��Stretch=�$��Stretch=�$
��Bold"��� -   $ 
sldPanning�� � Value$ 0Q�$��50�� �� IsSnapToTickEnabled . 
���   �$��Center=�$��20��$ -1Q�$ 1Q�$ 1��$��3���� $��R��$��20��$��Stretch=�$��Stretch=�$
��Bold"��� -   $ 
cbxMonitor��$��Monitor��$��20��$
��Bold"��� -   $ sldCalibSetpoint��
���   �$��Stretch=�$��20��$ 0Q�$
 1000Q��� $ 
Horizontal=�.+��� -   $	 btnCalibrateSines��$��Major��$��30��$��70��
�������$��2q�
�������$��Magneto)�$��16(�$��2,2,2,2q����� $$ Images/Backgrounds/MidWood.png��$
 Tile=�$

 None=�� $��1�� �� $��9* ��� $	��90* � �� $
��127* ��� $	��27* ��� $
��154* �.+��� $��0��$��3��$��2q�
�������$��	2,2,2,2.6q����� $&  Images/Backgrounds/LightWood.jpg��$
 Tile=�._��  � Orientation$ 
Horizontal=�.+��� -   $	 
btnSineCal��$��Calib��$��30��$��70��
�������$��2q�
�������$��Magneto)�$��16(�$��2,2,2,2q����� $$ Images/Backgrounds/MidWood.png��$
 Tile=�$

 None=��� -    $ lblSineCalibrationLed��$��30��$��30��
���   �$��Center=�
�������$��2q��� -!   $ lblAllNotes��$��
All Notes:��$��15(�$
��Bold"�$��SegoeUI)�$��30��$��83��
������ $��Center=�
������ $��2q��� -"    ��Name$ brdNanRadio��$��30��$��30��
���`���
�������$��2q��� -#   $	 btnSaveCalib��$��
Save calib��$��30��$	��110��
�������$��2q�
�������$��Magneto)�$��16(�$��2,2,2,2q����� $$ Images/Backgrounds/MidWood.png��$
 Tile=�$

 None=��� -$   $	 btnLoadCalib��$��
Load calib��$��30��$	��110��
�������$��2q�
�������$��Magneto)�$��16(�$��2,2,2,2q����� $$ Images/Backgrounds/MidWood.png��$
 Tile=�$

 None=��� $��0��$��3��$��1��$��2q�
�������$��2,2.4,2,2.6q�._��� -%    ��Name$ cnvsPlot��
���   �$��Stretch=�$��Stretch=��� $��2��$��2q�
�������$��2,1.8,2,1.6q�$��2������ $$ Images/Backgrounds/MidWood.png��$
 Tile=�._�� -&    �Name$ gridKeys�� �� $��3* ��� $��2* ��� $��8* ��� $��2* �