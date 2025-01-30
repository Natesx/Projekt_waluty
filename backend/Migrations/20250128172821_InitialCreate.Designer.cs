﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace backend.Migrations
{
    [DbContext(typeof(CurrencyExchangeContext))]
    [Migration("20250128172821_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.1");

            modelBuilder.Entity("Rate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Mid")
                        .HasColumnType("TEXT");

                    b.Property<int>("TableId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("TableId", "Code")
                        .IsUnique();

                    b.ToTable("Rates", (string)null);
                });

            modelBuilder.Entity("TableA", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("EffectiveDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("No")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TableName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("TableA", (string)null);
                });

            modelBuilder.Entity("Rate", b =>
                {
                    b.HasOne("TableA", "Table")
                        .WithMany("Rates")
                        .HasForeignKey("TableId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Table");
                });

            modelBuilder.Entity("TableA", b =>
                {
                    b.Navigation("Rates");
                });
#pragma warning restore 612, 618
        }
    }
}
