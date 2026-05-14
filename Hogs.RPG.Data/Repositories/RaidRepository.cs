using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Data;
using Microsoft.EntityFrameworkCore;

namespace Hogs.RPG.Data.Repositories
{
    public class RaidRepository
    {
        private readonly GameDbContext _context;

        public RaidRepository(GameDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET SESSION WITH PARTICIPANTS
        // =========================
        public async Task<RaidSession?> GetSessionAsync(int sessionId)
        {
            var session = await _context.RaidSessions
                .Include(s => s.Participants)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            session?.DeserializeEffects();
            return session;
        }

        // =========================
        // GET ACTIVE LOBBY BY CHANNEL
        // =========================
        public async Task<RaidSession?> GetLobbyByChannelAsync(ulong channelId)
        {
            var session = await _context.RaidSessions
                .Include(s => s.Participants)
                .FirstOrDefaultAsync(s => s.LobbyChannelId == channelId
                    && s.Status == RaidStatus.Lobby);

            session?.DeserializeEffects();
            return session;
        }

        // =========================
        // GET ACTIVE RAID BY THREAD
        // =========================
        public async Task<RaidSession?> GetActiveByThreadAsync(ulong threadId)
        {
            var session = await _context.RaidSessions
                .Include(s => s.Participants)
                .FirstOrDefaultAsync(s => s.ThreadId == threadId
                    && s.Status == RaidStatus.Active);

            session?.DeserializeEffects();
            return session;
        }

        // =========================
        // GET PLAYER'S ACTIVE SESSION
        // =========================
        public async Task<RaidSession?> GetPlayerActiveSessionAsync(ulong discordId)
        {
            var session = await _context.RaidSessions
                .Include(s => s.Participants)
                .FirstOrDefaultAsync(s =>
                    (s.Status == RaidStatus.Lobby || s.Status == RaidStatus.Active)
                    && s.Participants.Any(p => p.DiscordId == discordId));

            session?.DeserializeEffects();
            return session;
        }

        // =========================
        // CREATE SESSION
        // =========================
        public async Task<RaidSession> CreateSessionAsync(RaidSession session)
        {
            session.SerializeEffects();
            _context.RaidSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        // =========================
        // ADD PARTICIPANT
        // =========================
        public async Task AddParticipantAsync(RaidParticipant participant)
        {
            _context.RaidParticipants.Add(participant);
            await _context.SaveChangesAsync();
        }

        // =========================
        // REMOVE PARTICIPANT
        // =========================
        public async Task RemoveParticipantAsync(int participantId)
        {
            var participant = await _context.RaidParticipants
                .FirstOrDefaultAsync(p => p.Id == participantId);

            if (participant != null)
            {
                _context.RaidParticipants.Remove(participant);
                await _context.SaveChangesAsync();
            }
        }

        // =========================
        // SAVE SESSION
        // =========================
        public async Task SaveSessionAsync(RaidSession session)
        {
            session.SerializeEffects();
            _context.RaidSessions.Update(session);
            await _context.SaveChangesAsync();
        }

        // =========================
        // DELETE SESSION
        // =========================
        public async Task DeleteSessionAsync(int sessionId)
        {
            var session = await _context.RaidSessions
                .Include(s => s.Participants)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session != null)
            {
                _context.RaidSessions.Remove(session);
                await _context.SaveChangesAsync();
            }
        }
    }
}