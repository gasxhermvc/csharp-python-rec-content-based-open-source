﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RecommendSystemContentBased.Models.TaiplaModel
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class taipla_wsEntities : DbContext
    {
        public taipla_wsEntities()
            : base("name=taipla_wsEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<comment> comment { get; set; }
        public virtual DbSet<food_center> food_center { get; set; }
        public virtual DbSet<food_country> food_country { get; set; }
        public virtual DbSet<food_culture> food_culture { get; set; }
        public virtual DbSet<food_ingredient> food_ingredient { get; set; }
        public virtual DbSet<history_search> history_search { get; set; }
        public virtual DbSet<legend> legend { get; set; }
        public virtual DbSet<media> media { get; set; }
        public virtual DbSet<promotion> promotion { get; set; }
        public virtual DbSet<restaurant> restaurant { get; set; }
        public virtual DbSet<restaurant_ingredient> restaurant_ingredient { get; set; }
        public virtual DbSet<restaurant_menu> restaurant_menu { get; set; }
        public virtual DbSet<user> user { get; set; }
        public virtual DbSet<user_device> user_device { get; set; }
        public virtual DbSet<vote> vote { get; set; }
    }
}
