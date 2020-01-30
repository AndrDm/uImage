These two indictor I'll place into dock containers

````xml
<ControlTemplate TargetType="{x:Type local:uImageControl}">
		            <DockPanel LastChildFill="True">
			            <DockPanel DockPanel.Dock="Bottom">                                        
				            <TextBox  Name="PART_µZoom" DockPanel.Dock="Left">Zoom</TextBox>
				            <TextBox  Name="PART_µInfo" DockPanel.Dock="Left">Zoom</TextBox>
			            </DockPanel>                    
                        <ScrollViewer Name="PART_µScrollViewer"  HorizontalScrollBarVisibility="Visible"> 
                            <Grid>
                                <Border Background="{TemplateBinding Background}"
                                        BorderBrush="{TemplateBinding BorderBrush}"
                                        BorderThickness="{TemplateBinding BorderThickness}">
                                </Border>
                                <Image Name="PART_µImage" Height="512" Width="512" Source="Zippo.jpg" />
                                <Grid Name="PART_µMouseHandler" Background="#00FFFFFF"/>
                            </Grid>
                        </ScrollViewer>
		            </DockPanel>                    
                </ControlTemplate>
````
