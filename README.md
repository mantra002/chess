# Basic Chess Engine

This is an attempt to create a functional, if slow/mediocre, chess engine. It currently is using mailbox board represenation and a very inefficent attack map which contains piece and location data.

The search is based on the Principal Variation search on the chess programming wiki (https://www.chessprogramming.org/Principal_Variation_Search). An attempt at Static Excange Evaluation was made.

To make it easier to play I've partially implemented a UCI interface. It's at least enought to load into Chessbase and play around.