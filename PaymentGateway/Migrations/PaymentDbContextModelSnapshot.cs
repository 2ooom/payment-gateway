﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PaymentGateway.Model;

namespace PaymentGateway.Migrations
{
    [DbContext(typeof(PaymentDbContext))]
    partial class PaymentDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.1");

            modelBuilder.Entity("PaymentGateway.Model.Merchant", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Url")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Merchants");
                });

            modelBuilder.Entity("PaymentGateway.Model.Payment", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("Amount")
                        .HasColumnType("REAL");

                    b.Property<string>("Currency")
                        .HasColumnType("TEXT")
                        .HasMaxLength(3);

                    b.Property<string>("Last4Digits")
                        .HasColumnType("TEXT");

                    b.Property<int>("MerchantId")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("MerchantId1")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("MerchantId1");

                    b.ToTable("Payments");
                });

            modelBuilder.Entity("PaymentGateway.Model.Payment", b =>
                {
                    b.HasOne("PaymentGateway.Model.Merchant", "Merchant")
                        .WithMany()
                        .HasForeignKey("MerchantId1");
                });
#pragma warning restore 612, 618
        }
    }
}
