   M S B A M L   `   `   `  ����J  FStrafrunden, Version=1.5.14.1806, Culture=neutral, PublicKeyToken=null)clr-namespace:StrafrundenStrafrunden  5   	   K*clr-namespace:Strafrunden.MarkupExtensionsStrafrunden.MarkupExtensions  5   	       Strafrunden.Transponder   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   +localclr-namespace:Strafrunden     5   	   :ext*clr-namespace:Strafrunden.MarkupExtensions     5   	     � Title$  Transponder��5	   	   $	��450��6   $	��500��6*    � WindowStyle$ 
ToolWindow=�66   .��5
      �  � ColumnDefinitions 5   
   �� 5      $��* �6   6+   �� 5      $��* �6   6+    � RowDefinitions 5   
   �� 5      $��20 �6   6*   �� 5      $��* �6   6)   �� 5      $��20 �6   6*   .+�5   
   o� $��2��6   $	��Top=�6#   .@�5      m� $��Ansicht��6   .@�5      m� -   5        $��Aktualisieren��5      6K   m� 5      -   5        $��AutoUpdateMenuItem��5   b   $��Automatisch Aktualisieren��6    m� IsCheckable .  m� 	IsChecked . 6~   5      m� 5      $��Einstellungen��6   .@�5      m� $��SourceName Filter RegEx��6   �� 5      $��150px��6   ��6+   :   4Strafrunden.MarkupExtensions.SettingBindingExtension  *RaceResultBoxNameFilter+6a   5      5      &   System.Windows.Controls.DataGrid  5   
   -   5        $��
DataOutput��5   0   $��0��6   $��1��6#      AutoGenerateColumns .  $ �� IsSynchronizedWithCurrentItem$
 TrueY�6^      Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5       ,  &System.Windows.Controls.DataGridColumn	   Header$	 Startnummer��6%   1  +System.Windows.Controls.DataGridBoundColumn
   Binding
 6:   �� *[0]+  5!      $	 Registrierungen��6%   
 6>   �� *[1]+5#        5$   
   -   5        $��DataOutput2��5$   0   $��1��6   $��1��6#    .  $
 TrueY�6_    5%        5&      $	 Startnummer��6%   
 6:   �� *[0]+  5'      $	 Fehlende Strafrunden��6%   
 6C   �� *[1]+5)      �� 5*   
   $��2��6   $��20��6!   $��10(�6-   $��Bottom=�6;   .@�5+      �� $��Letztes Update:��6   69   �� 5,      -   5        $��LastUpdateStatus��5,      67   5-      5.      5/      