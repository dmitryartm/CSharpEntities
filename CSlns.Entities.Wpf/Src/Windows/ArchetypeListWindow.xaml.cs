﻿using System.Windows;
using System.Windows.Media;
using CSlns.Entities.Wpf.ViewModels;


namespace CSlns.Entities.Wpf.Windows {
    public partial class ArchetypeListWindow : Window {
        public ArchetypeListWindow(EntityManager entityManager) {
            this.InitializeComponent();

            this.archetypeListView.ViewModel = new ArchetypeListViewModel(entityManager);


            var i = 0; 
            
            CompositionTarget.Rendering += (_, __) => {
                ++i;
                
                this.archetypeListView.ViewModel.Update();
                
                entityManager.CreateEntity<float, double>();

                if (i % 100 == 0) {
                    entityManager.CreateEntity<int, float, double>();
                }
            };
        }
    }
}