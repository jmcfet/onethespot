   M S B A M L   `   `   `  ����>  :Led, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null4%clr-namespace:LedControl;assembly=Led
LedControl  5   
    	OnTheSpot   OnTheSpot.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   4uc%clr-namespace:LedControl;assembly=Led     5   
     � Title$  On The Spot QSS��5   	   $	��800��6!   $	��800��6.    � 	Resources 5          BooleanToVisibilityConverter��%   &	        �� 5   
   6'   5      .��5	      � -   5         �Name$ 
LayoutRoot��5	      ���	#FF0C0C0C � RowDefinitions 5
   
   �� 5      $��50 �6   6*   �� 5      $��* �6   6)   .+�5   
   � $��0��6   ��5      �� 5       �� EndPoint$ 0.5,1&�6&    �� 
StartPoint$ 0.5,0&�67   .��5      �  � Color$ 	#FFFFFFFF��6#    � Offset$ 1Q�65   � 5      $ 	#FF5E5757��6#   5      .+�5      ��  �� Orientation$ 
Horizontal=�6   $��0��62   .+�5      �� -   5        $��label1��5   7   $��Phidgit��6   $��31��6+   $��15(�6E   $��60��6S   $y�	Collapsed=�6^      LedControl.Led  5      -   5        	  Name$
	 led1��5      
   Flashing
 .     FlashingPeriod$	 500��68      	ColorNull$
 Gray��6M      ColorOn$ Green��6^      ColorOff$	 Red��6n   $y�	Collapsed=�6~   �� 5      -   5        $��label2��5   E   $��Database��6   $��15(�6+   $��31��69   $��73��6S     5      -   5        $
	 led2��5      $
 Gray��6(   $ Green��69   $	 Red��6I   6[   �� 5      -   5        $��label4��5   7   $��Password��6   $��31��6+   $��75��6E   $��15(�6Q   $y�	Collapsed=�6_   G� 5      -   5        $��passwordBox1��5   +   $��23��6   $	��120��6?   $y�	Collapsed=�6K   �� 5      -   5        $��passwordEntered��5   1   $��>��6   $��21��6%   $��36��6H   $y�	Collapsed=�6r   �� 5       -	   5        $��version��5    7   $��Version��6   $��21��6+   $��67��6F   $��12q�6Q   6]   �� 5!      -
   5        $��Classify��5!   L   $��40��6@   $y�	Collapsed=�6\   $��77��6s   ��6   �� *ClassifyButtonText+6   �� 5"      -   5        $��
ReClassify��5"   N   $��40��6B   $y�	Collapsed=�6`   $��88��6w   ��6   �� *ReClassifyButtonText+6�   �� 5#      -   5        $��QuickReClassify��5#   S   $��40��6G   $y�	Collapsed=�6j   $	��120��6�   ��6   �� *QuickReClassifyButtonText+6�   5$      5%      � 5&   
   $��1��6    � ColumnDefinitions 5'      �� 5(      $��* �6#   .+�5+      � -   5        $ Views��5+   #   $��0��6   .+�5,      �� -   5        $��
DBErrorMsg��5,   W   $��53��6   $
��Left=�6(   $��
373,11,0,0q�6C   
�������$�� ��6|   $��20(�6�   $
��Wrap=�6�   $	��Top=�6�   $	��184��6�   �� 5-      -   5        $��
textBlock1��5-   V   $��32��6   $
��Left=�6(   $��	96,11,0,0q�6C   $(��"System initializing .. please wait��6h   $��15(�6�   
�������$	��Top=�6�   $	��282��6�   5.      50      51      52      