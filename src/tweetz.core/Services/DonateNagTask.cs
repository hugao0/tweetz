﻿using System.Threading.Tasks;
using tweetz.core.Models;

namespace tweetz.core.Services
{
    public static class DonateNagTask
    {
        private const int donateNagCounterInterval = 120;
        private static int donateNagCounter = donateNagCounterInterval - 10;

        public static async Task Execute(TwitterTimeline timeline)
        {
            if (timeline.Settings.Donated) return;

            if (donateNagCounter >= donateNagCounterInterval)
            {
                donateNagCounter = 0;
                timeline.StatusCollection.Insert(0, new DonateNagStatus());
            }
            else
            {
                donateNagCounter += 1;
            }

            await Task.CompletedTask;
        }
    }
}