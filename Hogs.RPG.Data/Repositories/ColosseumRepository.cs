using Hogs.RPG.Core.Entities.ColosseumObjects;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Data;
using Microsoft.EntityFrameworkCore;

namespace Hogs.RPG.Data.Repositories
{
    public class ColosseumRepository
    {
        private readonly GameDbContext _context;

        public ColosseumRepository(GameDbContext context)
        {
            _context = context;
        }

        // =========================
        // TOURNAMENT
        // =========================
        public async Task<ColosseumTournament> CreateTournamentAsync(ColosseumTournament tournament)
        {
            _context.ColosseumTournaments.Add(tournament);
            await _context.SaveChangesAsync();
            return tournament;
        }

        public async Task<ColosseumTournament?> GetTournamentAsync(int tournamentId)
        {
            return await _context.ColosseumTournaments
                .Include(t => t.Participants)
                    .ThenInclude(p => p.Build)
                .Include(t => t.Matches)
                .FirstOrDefaultAsync(t => t.Id == tournamentId);
        }

        // Used by the scheduler to find the tournament currently taking
        // signups, and by the signup command to validate against it.
        public async Task<ColosseumTournament?> GetActiveRegistrationAsync()
        {
            return await _context.ColosseumTournaments
                .Include(t => t.Participants)
                    .ThenInclude(p => p.Build)
                .FirstOrDefaultAsync(t => t.Status == ColosseumTournamentStatus.Registration);
        }

        // Used by the scheduler to find whatever tournament is currently
        // mid-bracket so it can advance the next round.
        public async Task<ColosseumTournament?> GetInProgressTournamentAsync()
        {
            return await _context.ColosseumTournaments
                .Include(t => t.Participants)
                    .ThenInclude(p => p.Build)
                .Include(t => t.Matches)
                .FirstOrDefaultAsync(t => t.Status == ColosseumTournamentStatus.InProgress);
        }

        public async Task SaveTournamentAsync(ColosseumTournament tournament)
        {
            _context.ColosseumTournaments.Update(tournament);
            await _context.SaveChangesAsync();
        }

        // =========================
        // PARTICIPANT
        // =========================
        public async Task<ColosseumParticipant> AddParticipantAsync(ColosseumParticipant participant)
        {
            _context.ColosseumParticipants.Add(participant);
            await _context.SaveChangesAsync();
            return participant;
        }

        public async Task<ColosseumParticipant?> GetParticipantAsync(int participantId)
        {
            return await _context.ColosseumParticipants
                .Include(p => p.Build)
                .FirstOrDefaultAsync(p => p.Id == participantId);
        }

        // Finds a real player's participant entry in whichever tournament is
        // currently open for registration - used to block double signup and
        // to route a DM build-flow interaction back to the right build.
        public async Task<ColosseumParticipant?> GetActiveParticipantByDiscordIdAsync(ulong discordId)
        {
            return await _context.ColosseumParticipants
                .Include(p => p.Build)
                .Include(p => p.ColosseumTournament)
                .FirstOrDefaultAsync(p =>
                    p.DiscordId == discordId
                    && !p.IsBot
                    && (p.ColosseumTournament.Status == ColosseumTournamentStatus.Registration
                        || p.ColosseumTournament.Status == ColosseumTournamentStatus.Locked
                        || p.ColosseumTournament.Status == ColosseumTournamentStatus.InProgress));
        }

        public async Task SaveParticipantAsync(ColosseumParticipant participant)
        {
            _context.ColosseumParticipants.Update(participant);
            await _context.SaveChangesAsync();
        }

        // =========================
        // BUILD
        // =========================
        public async Task<ColosseumBuild> CreateBuildAsync(ColosseumBuild build)
        {
            _context.ColosseumBuilds.Add(build);
            await _context.SaveChangesAsync();
            return build;
        }

        public async Task SaveBuildAsync(ColosseumBuild build)
        {
            _context.ColosseumBuilds.Update(build);
            await _context.SaveChangesAsync();
        }

        // =========================
        // MATCH
        // =========================
        public async Task<ColosseumMatch> CreateMatchAsync(ColosseumMatch match)
        {
            _context.ColosseumMatches.Add(match);
            await _context.SaveChangesAsync();
            return match;
        }

        public async Task<ColosseumMatch?> GetMatchAsync(int matchId)
        {
            return await _context.ColosseumMatches
                .Include(m => m.ParticipantA)
                .Include(m => m.ParticipantB)
                .FirstOrDefaultAsync(m => m.Id == matchId);
        }

        public async Task<ColosseumMatch?> GetMatchByThreadAsync(ulong threadId)
        {
            return await _context.ColosseumMatches
                .Include(m => m.ParticipantA)
                .Include(m => m.ParticipantB)
                .FirstOrDefaultAsync(m => m.ThreadId == threadId);
        }

        // All matches for one tournament, ordered so bracket advancement
        // logic can walk them round by round within each bracket type.
        public async Task<List<ColosseumMatch>> GetMatchesForTournamentAsync(int tournamentId)
        {
            return await _context.ColosseumMatches
                .Include(m => m.ParticipantA)
                .Include(m => m.ParticipantB)
                .Where(m => m.ColosseumTournamentId == tournamentId)
                .OrderBy(m => m.BracketType)
                .ThenBy(m => m.RoundNumber)
                .ToListAsync();
        }

        public async Task SaveMatchAsync(ColosseumMatch match)
        {
            _context.ColosseumMatches.Update(match);
            await _context.SaveChangesAsync();
        }
    }
}