   M S B A M L   `   `   `  ����J  FStrafrunden, Version=1.5.14.1806, Culture=neutral, PublicKeyToken=null)clr-namespace:StrafrundenStrafrunden  5   	   (    "Strafrunden.TransponderEntryDialog   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   +localclr-namespace:Strafrunden     5   	     � Title$  Transponderauswahl��5   	   $	��150��6$   $	��225��61    � WindowStyle$ 
ToolWindow=�6=   .��5	      �  � RowDefinitions 5
   
   .+�5   
   �� $��	10,10,0,0q�6   $
��Left=�6#   $��84��6>   $��28��6I   $	��Top=�6U   .��6m   Code:6t   �� 5   
   -   5        $��CodeBox��5      $��
99,10,10,0q�6h   $��22��6|   $	��Top=�6�   ��6!   �� *Code+ �� Mode$ TwoWay=� �� 
IsReadOnly 6D   �� *CodeDisabled+6�   �� 5   
   $��	10,37,0,0q�6   $
��Left=�6#   $��89��6>   $��28��6I   $	��Top=�6U   .��6m   Startnummer:6{   �� 5   
   -   5        $��StarnummerBox��5      $��
99,37,10,0q�6|   $��22��6�   $	��Top=�6�   ��6'   �� *Startnummer+$ TwoWay=� 6Q   �� *StartnummerDisabled+6�   �� 5   
   -   5        $��OK��5   �    �� 	IsDefault . $��OK��6"   $��	0,0,10,10q�6/   $��2��6B   $��Right=�6S   $��80��6o   $��26��6z   $��Bottom=�6�   6�   �� 5   
   -   5        $��Abort��5   5    �� IsCancel . $��	Abbrechen��6!   $��	10,0,0,10q�6B   $
��Left=�6U   $��89��6p   $��26��6{   $��Bottom=�6�   6�   5      5      