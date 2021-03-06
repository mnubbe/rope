/*
 * Singleton to track stats for the entire program.
 */

using System;
using System.Collections.Generic;

using ClassLibrary1.Collections.Generic;

namespace Statistics
{
    /// <summary>
    /// Singleton bean-counter class. Currently only tracks frames per second.
    /// </summary>
    class Stats
    {
        private static Stats instance;
        public static Stats Instance()
        {
            if (instance == null) {
                instance = new Stats();
            }
            return instance;
        }


        private const int FPS_WINDOW = 150;
        private Dictionary<string, RingBuffer<int>> fps = new Dictionary<string,
                RingBuffer<int>>();


        /// <summary>
        /// Registers one tick for the given name.
        /// </summary>
        /// <param name="name">A string identifying the caller.</param>
        /// <param name="value">The new value.</param>
        public void AddValue(string name, int val)
        {
            RingBuffer<int> rb;
            if (fps.ContainsKey(name)) {
                rb = fps[name];
            } else {
                rb = new RingBuffer<int>(FPS_WINDOW);
                // Fill the RingBuffer with sentinel values so we don't have to
                // do extra bookkeeping.
                for (int i = 0; i < FPS_WINDOW; i++) {
                    rb.Add(-1);
                }
                fps.Add(name, rb);
            }

            lock (rb) {
                rb.Add(val);
            }
        }


        /// <summary>
        /// Computes the framerate at which a component is operating.
        /// </summary>
        /// <param name="name">A string identifying which component to report
        /// about.</param>
        /// <returns>A double representing the frames per second the component
        /// is handling.</returns>
        /// <exception cref="ArgumentException">Thrown when name has not been
        /// used in a call to GetFrameTickBuffer().</exception>
        public double GetFPS(string name)
        {
            if (fps.ContainsKey(name) == false) {
                throw new ArgumentException("Frame tick buffer for " + name
                        + " does not exist!");
            }

            int sum = 0;
            int values = 0;
            RingBuffer<int> rb = fps[name];
            lock (rb) {
                foreach (int i in rb) {
                    if (i >= 0) {
                        sum += i;
                        values++;
                    }
                }
            }

            double framerate = (double)(values / (((double)sum) / 1000));
            return framerate;
        }
    }


    /// <summary>
    /// Helper class for Stats' FPS buffers. Instantiate a Tick when you start
    /// working, and Add(tick.Tock()) to your buffer when you're done.
    /// </summary>
    class Tick
    {
        private DateTime start;


        /// <summary>
        /// Starts counting on instantiation.
        /// </summary>
        public Tick()
        {
            start = DateTime.Now;
        }


        /// <returns>The number of milliseconds that have elapsed since this was
        /// instantiated.</returns>
        public int Tock()
        {
            return (DateTime.Now - start).Milliseconds;
        }
    }
}

