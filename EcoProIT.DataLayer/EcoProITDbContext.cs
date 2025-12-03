using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EcoProIT.DataLayer
{
    /// <summary>
    /// Entity Framework Core DbContext for EcoProIT application using SQLite
    /// </summary>
    public class EcoProITDbContext : DbContext
    {
        public EcoProITDbContext()
        {
        }

        public EcoProITDbContext(DbContextOptions<EcoProITDbContext> options)
            : base(options)
        {
        }

        public DbSet<ConsumableBase> ConsumableBase { get; set; }
        public DbSet<ConsumablesEmission> ConsumablesEmission { get; set; }
        public DbSet<Emissions> Emissions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Default SQLite database location
                var dbPath = System.IO.Path.Combine(
                    System.AppDomain.CurrentDomain.BaseDirectory,
                    "Resources",
                    "modeloutput.db");

                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ConsumableBase
            modelBuilder.Entity<ConsumableBase>(entity =>
            {
                entity.ToTable("ConsumableBase");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Name).HasMaxLength(2000);
                entity.Property(e => e.Unit).HasMaxLength(1000);
            });

            // Configure Emissions
            modelBuilder.Entity<Emissions>(entity =>
            {
                entity.ToTable("Emissions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Name).HasMaxLength(2000);
                entity.Property(e => e.Unit).HasMaxLength(100);
            });

            // Configure ConsumablesEmission (many-to-many relationship table)
            modelBuilder.Entity<ConsumablesEmission>(entity =>
            {
                entity.ToTable("ConsumablesEmission");
                entity.HasKey(e => new { e.Consumable, e.Emission });

                entity.HasOne(ce => ce.ConsumableBase)
                    .WithMany(c => c.ConsumablesEmission)
                    .HasForeignKey(ce => ce.Consumable)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ce => ce.Emissions)
                    .WithMany(e => e.ConsumablesEmission)
                    .HasForeignKey(ce => ce.Emission)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }

    // Entity classes remain mostly the same but with EF Core conventions
    public partial class ConsumableBase : INotifyPropertyChanging, INotifyPropertyChanged
    {
        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(string.Empty);

        private int _Id;
        private string _Name;
        private string _Unit;

        public ConsumableBase()
        {
            ConsumablesEmission = new HashSet<ConsumablesEmission>();
            OnCreated();
        }

        public int Id
        {
            get => _Id;
            set
            {
                if (_Id != value)
                {
                    OnIdChanging(value);
                    SendPropertyChanging();
                    _Id = value;
                    SendPropertyChanged("Id");
                    OnIdChanged();
                }
            }
        }

        public string Name
        {
            get => _Name;
            set
            {
                if (_Name != value)
                {
                    OnNameChanging(value);
                    SendPropertyChanging();
                    _Name = value;
                    SendPropertyChanged("Name");
                    OnNameChanged();
                }
            }
        }

        public string Unit
        {
            get => _Unit;
            set
            {
                if (_Unit != value)
                {
                    OnUnitChanging(value);
                    SendPropertyChanging();
                    _Unit = value;
                    SendPropertyChanged("Unit");
                    OnUnitChanged();
                }
            }
        }

        public virtual ICollection<ConsumablesEmission> ConsumablesEmission { get; set; }

        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void SendPropertyChanging()
        {
            PropertyChanging?.Invoke(this, emptyChangingEventArgs);
        }

        protected virtual void SendPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        partial void OnLoaded();
        partial void OnCreated();
        partial void OnIdChanging(int value);
        partial void OnIdChanged();
        partial void OnNameChanging(string value);
        partial void OnNameChanged();
        partial void OnUnitChanging(string value);
        partial void OnUnitChanged();
    }

    public partial class ConsumablesEmission : INotifyPropertyChanging, INotifyPropertyChanged
    {
        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(string.Empty);

        private int _Consumable;
        private int _Emission;
        private double? _Value;

        public ConsumablesEmission()
        {
            OnCreated();
        }

        public int Consumable
        {
            get => _Consumable;
            set
            {
                if (_Consumable != value)
                {
                    OnConsumableChanging(value);
                    SendPropertyChanging();
                    _Consumable = value;
                    SendPropertyChanged("Consumable");
                    OnConsumableChanged();
                }
            }
        }

        public int Emission
        {
            get => _Emission;
            set
            {
                if (_Emission != value)
                {
                    OnEmissionChanging(value);
                    SendPropertyChanging();
                    _Emission = value;
                    SendPropertyChanged("Emission");
                    OnEmissionChanged();
                }
            }
        }

        public double? Value
        {
            get => _Value;
            set
            {
                if (_Value != value)
                {
                    OnValueChanging(value);
                    SendPropertyChanging();
                    _Value = value;
                    SendPropertyChanged("Value");
                    OnValueChanged();
                }
            }
        }

        public virtual ConsumableBase ConsumableBase { get; set; }
        public virtual Emissions Emissions { get; set; }

        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void SendPropertyChanging()
        {
            PropertyChanging?.Invoke(this, emptyChangingEventArgs);
        }

        protected virtual void SendPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        partial void OnLoaded();
        partial void OnCreated();
        partial void OnConsumableChanging(int value);
        partial void OnConsumableChanged();
        partial void OnEmissionChanging(int value);
        partial void OnEmissionChanged();
        partial void OnValueChanging(double? value);
        partial void OnValueChanged();
    }

    public partial class Emissions : INotifyPropertyChanging, INotifyPropertyChanged
    {
        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(string.Empty);

        private int _Id;
        private string _Name;
        private string _Unit;

        public Emissions()
        {
            ConsumablesEmission = new HashSet<ConsumablesEmission>();
            OnCreated();
        }

        public int Id
        {
            get => _Id;
            set
            {
                if (_Id != value)
                {
                    OnIdChanging(value);
                    SendPropertyChanging();
                    _Id = value;
                    SendPropertyChanged("Id");
                    OnIdChanged();
                }
            }
        }

        public string Name
        {
            get => _Name;
            set
            {
                if (_Name != value)
                {
                    OnNameChanging(value);
                    SendPropertyChanging();
                    _Name = value;
                    SendPropertyChanged("Name");
                    OnNameChanged();
                }
            }
        }

        public string Unit
        {
            get => _Unit;
            set
            {
                if (_Unit != value)
                {
                    OnUnitChanging(value);
                    SendPropertyChanging();
                    _Unit = value;
                    SendPropertyChanged("Unit");
                    OnUnitChanged();
                }
            }
        }

        public virtual ICollection<ConsumablesEmission> ConsumablesEmission { get; set; }

        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void SendPropertyChanging()
        {
            PropertyChanging?.Invoke(this, emptyChangingEventArgs);
        }

        protected virtual void SendPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        partial void OnLoaded();
        partial void OnCreated();
        partial void OnIdChanging(int value);
        partial void OnIdChanged();
        partial void OnNameChanging(string value);
        partial void OnNameChanged();
        partial void OnUnitChanging(string value);
        partial void OnUnitChanged();
    }
}
