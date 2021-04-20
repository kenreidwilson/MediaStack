﻿// <auto-generated />

using System;
using MediaStackCore.Data_Access_Layer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MediaStackCore.Migrations
{
    [DbContext(typeof(MediaStackContext))]
    [Migration("20210317021450_media_score_and_source")]
    partial class media_score_and_source
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.4");

            modelBuilder.Entity("MediaStack_Library.Model.Album", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ArtistID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ArtistID");

                    b.HasIndex("Name", "ArtistID")
                        .IsUnique();

                    b.ToTable("Albums");
                });

            modelBuilder.Entity("MediaStack_Library.Model.Artist", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Artists");
                });

            modelBuilder.Entity("MediaStack_Library.Model.Category", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("MediaStack_Library.Model.Media", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("AlbumID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ArtistID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("CategoryID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<string>("Hash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Path")
                        .HasColumnType("TEXT");

                    b.Property<int>("Score")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Source")
                        .HasColumnType("TEXT");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("AlbumID");

                    b.HasIndex("ArtistID");

                    b.HasIndex("CategoryID");

                    b.HasIndex("Hash")
                        .IsUnique();

                    b.ToTable("Media");
                });

            modelBuilder.Entity("MediaStack_Library.Model.Tag", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("MediaID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("MediaID");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("MediaStack_Library.Model.Album", b =>
                {
                    b.HasOne("MediaStack_Library.Model.Artist", "Artist")
                        .WithMany()
                        .HasForeignKey("ArtistID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Artist");
                });

            modelBuilder.Entity("MediaStack_Library.Model.Media", b =>
                {
                    b.HasOne("MediaStack_Library.Model.Album", "Album")
                        .WithMany()
                        .HasForeignKey("AlbumID");

                    b.HasOne("MediaStack_Library.Model.Artist", "Artist")
                        .WithMany()
                        .HasForeignKey("ArtistID");

                    b.HasOne("MediaStack_Library.Model.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryID");

                    b.Navigation("Album");

                    b.Navigation("Artist");

                    b.Navigation("Category");
                });

            modelBuilder.Entity("MediaStack_Library.Model.Tag", b =>
                {
                    b.HasOne("MediaStack_Library.Model.Media", null)
                        .WithMany("Tags")
                        .HasForeignKey("MediaID");
                });

            modelBuilder.Entity("MediaStack_Library.Model.Media", b =>
                {
                    b.Navigation("Tags");
                });
#pragma warning restore 612, 618
        }
    }
}
