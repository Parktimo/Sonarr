﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.SeriesStats
{
    public interface ISeriesStatisticsService
    {
        List<SeriesStatistics> SeriesStatistics();
        SeriesStatistics SeriesStatistics(int seriesId);
    }

    public class SeriesStatisticsService : ISeriesStatisticsService
    {
        private readonly ISeriesStatisticsRepository _seriesStatisticsRepository;

        public SeriesStatisticsService(ISeriesStatisticsRepository seriesStatisticsRepository)
        {
            _seriesStatisticsRepository = seriesStatisticsRepository;
        }

        public List<SeriesStatistics> SeriesStatistics()
        {
            var seasonStatistics = _seriesStatisticsRepository.SeriesStatistics();

            return seasonStatistics.GroupBy(s => s.SeriesId).Select(s => MapSeriesStatistics(s.ToList())).ToList();
        }

        public SeriesStatistics SeriesStatistics(int seriesId)
        {
            var stats = _seriesStatisticsRepository.SeriesStatistics(seriesId);

            if (stats == null || stats.Count == 0) return new SeriesStatistics();

            return MapSeriesStatistics(stats);
        }

        private SeriesStatistics MapSeriesStatistics(List<SeasonStatistics> seasonStatistics)
        {
            return new SeriesStatistics
                   {
                       SeasonStatistics = seasonStatistics,
                       SeriesId = seasonStatistics.First().SeriesId,
                       EpisodeFileCount = seasonStatistics.Sum(s => s.EpisodeFileCount),
                       EpisodeCount = seasonStatistics.Sum(s => s.EpisodeCount),
                       TotalEpisodeCount = seasonStatistics.Sum(s => s.TotalEpisodeCount),
                       SizeOnDisk = seasonStatistics.Sum(s => s.SizeOnDisk),
                       NextAiringString = seasonStatistics.OrderBy(s =>
                       {
                           DateTime nextAiring;

                           if (!DateTime.TryParse(s.NextAiringString, out nextAiring)) return DateTime.MinValue;

                           return nextAiring;
                       }).First().NextAiringString,

                       PreviousAiringString = seasonStatistics.OrderBy(s =>
                       {
                           DateTime nextAiring;

                           if (!DateTime.TryParse(s.PreviousAiringString, out nextAiring)) return DateTime.MinValue;

                           return nextAiring;
                       }).Last().PreviousAiringString
                   };
        }
    }
}
