   M S B A M L   `   `   `  ����K  GNetSemestrClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:NetSemestrClientNetSemestrClient  5   	   !    NetSemestrClient.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   0localclr-namespace:NetSemestrClient     5   	   -   5          � Title$  Клиент��5   	   $	��450��6   $	��800��6%   .��5	      � .+�5
   
   �� $��Адрес сервера��6   $
��Left=�6(   $��	30,51,0,0q�6C   $	��Top=�6V   �� 5   
   -   5         ��Name$ textBoxServerAddress��5      $
��Left=�60   $��23��6K   $��
126,54,0,0q�6W    �� TextWrapping$
 Wrap=�6k   $��25.87.202.171��6   $	��Top=�6�   $	��255��6�    J� RenderTransformOrigin$ 0.512,0.989&�6�   �� 5   
   $	��TCP��6   $
��Left=�6   $��
443,51,0,0q�69   $	��Top=�6M   �� 5   
   -   5        $ textBoxPort��5      $
��Left=�6'   $��23��6B   $��
492,54,0,0q�6N   $
 Wrap=�6b   $
��9999��6v   $	��Top=�6�   $��90��6�   �� 5   
   -   5         ��Name$ 
btnConnect��5      $��Подключиться��6%   $
��Left=�6<   $��
338,98,0,0q�6W   $	��Top=�6k   $	��108��6�   6�   &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ 
tableFiles��5      $
��Left=�6'   $	��211��6B   $��149,185,0,0q�6O   $	��Top=�6d   $	��500��6|      AutoGenerateColumns .     
IsReadOnly .    SelectionMode$ Single=�6�   ��6�   �� *files+	   Columns	 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn
   Header$
 Название��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$	 350 6U   1  +System.Windows.Controls.DataGridBoundColumn   Binding 67   ��  �� Path$
 Name�6U     5      $
 Размер��6%   $	 150 6U    65   �� $ Length�6U   5      �� 5   
   -   5        $ 
btnAddfile��5      $��Добавить��6%   $
��Left=�68   $��184,158,0,0q�6S   $	��Top=�6h   $��92��6�   {�.  6�   �� 5   
   -   5        $ btnRenamefile��5      $ ��Переименовать��6(   $
��Left=�6@   $��344,158,0,0q�6[   $	��Top=�6p   $	��102��6�   {�.  6�   �� 5   
   -   5        $ btnDeletefile��5      $��Удалить��6(   $
��Left=�6:   $��507,158,0,0q�6U   $	��Top=�6j   $	��101��6�   {�.  6�   �� 5   
   $��
Папка��6   $
��Left=�6    $��	36,12,0,0q�6;   $	��Top=�6N   �� 5   
   -	   5        $ textBoxDirPath��5      $
��Left=�6*   $��23��6E   $��
108,15,0,0q�6Q   $
 Wrap=�6e   $'��!C:\Users\User\Desktop\Test\Client��6y   $	��Top=�6�   $	��474��6�    �� 
IsReadOnly . 6�   �� 5   
   -
   5        $ btnChooseDir��5      $��Выбрать��6(   $
��Left=�6:   $��
631,16,0,0q�6U   $	��Top=�6i   $��76��6�   6�   �� 5   
   -   5        $ btnDisconnect��5      $��Отключиться��6(   $
��Left=�6>   $��
474,98,0,0q�6Y   $	��Top=�6m   $	��108��6�   {�.  6�   �� 5   
   -   5        $ textBoxNick��5      $
��Left=�6'   $��23��6B   $��	72,98,0,0q�6N   $
 Wrap=�6a   $��mynick��6u   $	��Top=�6�   $	��120��6�   �� 5   
   $��Ник��6   $
��Left=�6   $��	36,96,0,0q�69   $	��Top=�6L   �� 5   
   -   5        $ 
textBoxUDP��5      $
��Left=�6&   $��23��6A   $��
676,54,0,0q�6M   $
 Wrap=�6a   $	��Top=�6v   $��90��6�   �� 5   
   $	��UDP��6   $
��Left=�6   $��
631,54,0,0q�69   $	��Top=�6M   �� 5    
   -   5        $ btnGetTCPbyUDP��5       $��Узнать TCP��6)   $
��Left=�6>   $��
664,96,0,0q�6Y   $	��Top=�6m   $��82��6�   6�   5#      5$      