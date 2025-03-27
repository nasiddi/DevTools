﻿// <auto-generated />
using System;
using DevTools.Application.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DevTools.Application.Migrations
{
    [DbContext(typeof(DevToolsContext))]
    partial class DevToolsContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.12")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("DevTools.Application.Models.Citadels.Character", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("ActivationDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("CharacterNumber")
                        .HasColumnType("int");

                    b.Property<int>("CharacterType")
                        .HasColumnType("int");

                    b.Property<DateTime?>("DeactivationDate")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Characters");
                });

            modelBuilder.Entity("DevTools.Application.Models.Citadels.Game", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime?>("FinishTime")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("DevTools.Application.Models.Citadels.Hand", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("GameId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.ToTable("Hands");
                });

            modelBuilder.Entity("DevTools.Application.Models.Citadels.Player", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.HasKey("Id");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("DevTools.Application.Models.Citadels.PlayerResult", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("GameId")
                        .HasColumnType("int");

                    b.Property<bool>("HasWon")
                        .HasColumnType("bit");

                    b.Property<int>("PlayerId")
                        .HasColumnType("int");

                    b.Property<int>("Points")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.HasIndex("PlayerId");

                    b.ToTable("PlayerResults");
                });

            modelBuilder.Entity("DevTools.Application.Models.Citadels.Turn", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CharacterNumber")
                        .HasColumnType("int");

                    b.Property<int>("HandId")
                        .HasColumnType("int");

                    b.Property<int>("PlayerId")
                        .HasColumnType("int");

                    b.Property<int>("TargetCharacterNumber")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("HandId");

                    b.HasIndex("PlayerId");

                    b.ToTable("Turns");
                });

            modelBuilder.Entity("DevTools.Application.Models.File", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Bytes")
                        .HasColumnType("int");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)");

                    b.Property<string>("Guid")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.HasKey("Id");

                    b.HasIndex("Guid")
                        .IsUnique();

                    b.ToTable("Files");
                });

            modelBuilder.Entity("DevTools.Application.Models.Flags", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("Flag")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Flags");
                });

            modelBuilder.Entity("DevTools.Application.Models.HueColor", b =>
                {
                    b.Property<int>("HueId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("DefaultColor")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("HueId");

                    b.ToTable("HueColors");
                });

            modelBuilder.Entity("DevTools.Application.Models.Quiz.Answer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("AnswerText")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)");

                    b.Property<bool>("IsCorrect")
                        .HasColumnType("bit");

                    b.Property<bool>("IsInHalfJoker")
                        .HasColumnType("bit");

                    b.Property<bool>("IsSelectedByContestant")
                        .HasColumnType("bit");

                    b.Property<int>("QuestionId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("QuestionId");

                    b.ToTable("Answer");
                });

            modelBuilder.Entity("DevTools.Application.Models.Quiz.Joker", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("JokerType")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<int?>("QuestionIndex")
                        .HasColumnType("int");

                    b.Property<int>("QuizShowId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("QuizShowId");

                    b.ToTable("Joker");
                });

            modelBuilder.Entity("DevTools.Application.Models.Quiz.Question", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Amount")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<int>("Index")
                        .HasColumnType("int");

                    b.Property<bool>("IsLockedIn")
                        .HasColumnType("bit");

                    b.Property<string>("QuestionText")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)");

                    b.Property<int>("QuizShowId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("QuizShowId");

                    b.ToTable("Question");
                });

            modelBuilder.Entity("DevTools.Application.Models.Quiz.QuizShow", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<int>("QuestionIndex")
                        .HasColumnType("int");

                    b.Property<DateTime?>("QuestionStartTime")
                        .HasColumnType("datetime2");

                    b.Property<bool>("RegistrationIsOpen")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("QuizShows");
                });

            modelBuilder.Entity("DevTools.Application.Models.Quiz.Team", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<int>("QuizShowId")
                        .HasColumnType("int");

                    b.Property<Guid>("TeamId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("QuizShowId");

                    b.HasIndex("Name", "QuizShowId")
                        .IsUnique();

                    b.HasIndex("TeamId", "QuizShowId")
                        .IsUnique();

                    b.ToTable("Team");
                });

            modelBuilder.Entity("DevTools.Application.Models.Quiz.TeamAnswer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AnswerId")
                        .HasColumnType("int");

                    b.Property<int>("AnswerTimeMilliseconds")
                        .HasColumnType("int");

                    b.Property<bool>("IsCorrect")
                        .HasColumnType("bit");

                    b.Property<int>("QuestionIndex")
                        .HasColumnType("int");

                    b.Property<int>("TeamId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TeamId", "QuestionIndex", "AnswerId")
                        .IsUnique();

                    b.ToTable("TeamAnswer");
                });

            modelBuilder.Entity("DevTools.Application.Models.Quiz.TeamJoker", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("JokerType")
                        .HasColumnType("int");

                    b.Property<int?>("QuestionIndex")
                        .HasColumnType("int");

                    b.Property<int>("TeamId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TeamId");

                    b.ToTable("TeamJoker");
                });

            modelBuilder.Entity("DevTools.Application.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("bit");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.HasKey("Id");

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("DevTools.Application.Models.Citadels.Hand", b =>
                {
                    b.HasOne("DevTools.Application.Models.Citadels.Game", "Game")
                        .WithMany("Hands")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Game");
                });

            modelBuilder.Entity("DevTools.Application.Models.Citadels.PlayerResult", b =>
                {
                    b.HasOne("DevTools.Application.Models.Citadels.Game", "Game")
                        .WithMany("PlayerResults")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DevTools.Application.Models.Citadels.Player", "Player")
                        .WithMany("PlayerResults")
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Game");

                    b.Navigation("Player");
                });

            modelBuilder.Entity("DevTools.Application.Models.Citadels.Turn", b =>
                {
                    b.HasOne("DevTools.Application.Models.Citadels.Hand", "Hand")
                        .WithMany("Turns")
                        .HasForeignKey("HandId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DevTools.Application.Models.Citadels.Player", "Player")
                        .WithMany("Turns")
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Hand");

                    b.Navigation("Player");
                });

            modelBuilder.Entity("DevTools.Application.Models.Quiz.Answer", b =>
                {
                    b.HasOne("DevTools.Application.Models.Quiz.Question", null)
                        .WithMany("Answers")
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DevTools.Application.Models.Quiz.Joker", b =>
                {
                    b.HasOne("DevTools.Application.Models.Quiz.QuizShow", null)
                        .WithMany("Jokers")
                        .HasForeignKey("QuizShowId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DevTools.Application.Models.Quiz.Question", b =>
                {
                    b.HasOne("DevTools.Application.Models.Quiz.QuizShow", null)
                        .WithMany("Questions")
                        .HasForeignKey("QuizShowId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DevTools.Application.Models.Quiz.Team", b =>
                {
                    b.HasOne("DevTools.Application.Models.Quiz.QuizShow", null)
                        .WithMany("Teams")
                        .HasForeignKey("QuizShowId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DevTools.Application.Models.Quiz.TeamAnswer", b =>
                {
                    b.HasOne("DevTools.Application.Models.Quiz.Team", null)
                        .WithMany("Answers")
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DevTools.Application.Models.Quiz.TeamJoker", b =>
                {
                    b.HasOne("DevTools.Application.Models.Quiz.Team", null)
                        .WithMany("Jokers")
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DevTools.Application.Models.Citadels.Game", b =>
                {
                    b.Navigation("Hands");

                    b.Navigation("PlayerResults");
                });

            modelBuilder.Entity("DevTools.Application.Models.Citadels.Hand", b =>
                {
                    b.Navigation("Turns");
                });

            modelBuilder.Entity("DevTools.Application.Models.Citadels.Player", b =>
                {
                    b.Navigation("PlayerResults");

                    b.Navigation("Turns");
                });

            modelBuilder.Entity("DevTools.Application.Models.Quiz.Question", b =>
                {
                    b.Navigation("Answers");
                });

            modelBuilder.Entity("DevTools.Application.Models.Quiz.QuizShow", b =>
                {
                    b.Navigation("Jokers");

                    b.Navigation("Questions");

                    b.Navigation("Teams");
                });

            modelBuilder.Entity("DevTools.Application.Models.Quiz.Team", b =>
                {
                    b.Navigation("Answers");

                    b.Navigation("Jokers");
                });
#pragma warning restore 612, 618
        }
    }
}
