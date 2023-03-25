using NumeralDash.Entities;
using System.Collections.Generic;

namespace NumeralDash.Rules
{
    interface ICollectionRule
    {
        /// <summary>
        /// Foreground color.
        /// </summary>
        Color Color { get; }

        /// <summary>
        /// Rule title displayed to the player.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Next number to be collected.
        /// </summary>
        Number NextNumber { get; }

        /// <summary>
        /// All numbers to be collected from the map.
        /// </summary>
        Queue<Number> Numbers { get; }

        /// <summary>
        /// Removes the next number from the list.
        /// </summary>
        void Dequeue();
    }
}
