﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ShiraSubathonTracker.DAL;

#nullable disable

namespace ShiraSubathonTracker.DAL.Migrations
{
    [DbContext(typeof(TrackerDatabaseContext))]
    [Migration("20240809140918_AddMetricsEndpoint")]
    partial class AddMetricsEndpoint
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("ShiraSubathonTracker.DAL.Entities.Minecraft.MinecraftPlayer", b =>
                {
                    b.Property<string>("Uuid")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("PlayerName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Uuid");

                    b.ToTable("MinecraftPlayers");
                });

            modelBuilder.Entity("ShiraSubathonTracker.DAL.Entities.Minecraft.MinecraftPlayerSessions", b =>
                {
                    b.Property<string>("IpAddress")
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.Property<string>("Uuid")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTimeOffset>("SessionStartDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("SessionEndDate")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("IpAddress", "Uuid", "SessionStartDate");

                    b.HasIndex("Uuid");

                    b.ToTable("MinecraftPlayerSessions");
                });

            modelBuilder.Entity("ShiraSubathonTracker.DAL.Entities.Minecraft.MinecraftServer", b =>
                {
                    b.Property<string>("IpAddress")
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.Property<bool>("CurrentServer")
                        .HasColumnType("bit");

                    b.Property<string>("DnsName")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<DateTimeOffset>("LastSeenOnline")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("MessageOfTheDay")
                        .HasMaxLength(120)
                        .HasColumnType("nvarchar(120)");

                    b.Property<string>("MetricsEndpoint")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ServerStatus")
                        .HasColumnType("int");

                    b.Property<string>("Version")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("IpAddress");

                    b.HasIndex("Version");

                    b.ToTable("MinecraftServers");
                });

            modelBuilder.Entity("ShiraSubathonTracker.DAL.Entities.Minecraft.MinecraftVersion", b =>
                {
                    b.Property<string>("Version")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<int>("ServerProtocol")
                        .HasColumnType("int");

                    b.HasKey("Version");

                    b.ToTable("MinecraftVersions");
                });

            modelBuilder.Entity("ShiraSubathonTracker.DAL.Entities.Twitch.SubGift", b =>
                {
                    b.Property<string>("TwitchUsername")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTimeOffset>("DateOfGift")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("AmountGifted")
                        .HasColumnType("int");

                    b.Property<int>("SubTier")
                        .HasColumnType("int");

                    b.Property<int>("SubathonId")
                        .HasColumnType("int");

                    b.HasKey("TwitchUsername", "DateOfGift");

                    b.HasIndex("SubathonId");

                    b.ToTable("SubGifts");
                });

            modelBuilder.Entity("ShiraSubathonTracker.DAL.Entities.Twitch.Subathon", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("IsCurrentSubathon")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("Subathons");
                });

            modelBuilder.Entity("ShiraSubathonTracker.DAL.Entities.Users.JwtToken", b =>
                {
                    b.Property<string>("Username")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<bool>("IsBlocked")
                        .HasColumnType("bit");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Username");

                    b.ToTable("JwtTokens");
                });

            modelBuilder.Entity("ShiraSubathonTracker.DAL.Entities.Users.User", b =>
                {
                    b.Property<string>("Username")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("SecretKey")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.HasKey("Username");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ShiraSubathonTracker.DAL.Entities.Minecraft.MinecraftPlayerSessions", b =>
                {
                    b.HasOne("ShiraSubathonTracker.DAL.Entities.Minecraft.MinecraftServer", "Server")
                        .WithMany("PlayerSessions")
                        .HasForeignKey("IpAddress")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ShiraSubathonTracker.DAL.Entities.Minecraft.MinecraftPlayer", "Player")
                        .WithMany()
                        .HasForeignKey("Uuid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Player");

                    b.Navigation("Server");
                });

            modelBuilder.Entity("ShiraSubathonTracker.DAL.Entities.Minecraft.MinecraftServer", b =>
                {
                    b.HasOne("ShiraSubathonTracker.DAL.Entities.Minecraft.MinecraftVersion", "MinecraftVersion")
                        .WithMany("Servers")
                        .HasForeignKey("Version")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MinecraftVersion");
                });

            modelBuilder.Entity("ShiraSubathonTracker.DAL.Entities.Twitch.SubGift", b =>
                {
                    b.HasOne("ShiraSubathonTracker.DAL.Entities.Twitch.Subathon", "Subathon")
                        .WithMany("SubGifts")
                        .HasForeignKey("SubathonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Subathon");
                });

            modelBuilder.Entity("ShiraSubathonTracker.DAL.Entities.Users.JwtToken", b =>
                {
                    b.HasOne("ShiraSubathonTracker.DAL.Entities.Users.User", "user")
                        .WithMany()
                        .HasForeignKey("Username")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("user");
                });

            modelBuilder.Entity("ShiraSubathonTracker.DAL.Entities.Minecraft.MinecraftServer", b =>
                {
                    b.Navigation("PlayerSessions");
                });

            modelBuilder.Entity("ShiraSubathonTracker.DAL.Entities.Minecraft.MinecraftVersion", b =>
                {
                    b.Navigation("Servers");
                });

            modelBuilder.Entity("ShiraSubathonTracker.DAL.Entities.Twitch.Subathon", b =>
                {
                    b.Navigation("SubGifts");
                });
#pragma warning restore 612, 618
        }
    }
}
