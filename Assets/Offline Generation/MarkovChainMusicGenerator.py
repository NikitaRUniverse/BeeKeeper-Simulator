import music21
import numpy as np
from collections import defaultdict
import random
import matplotlib.pyplot as plt

class MarkovMelodyGenerator:
    def __init__(self, midi_file, state_size=3, melody_length=100, random_seed=None):
        self.midi_file = midi_file
        self.state_size = state_size
        self.melody_length = melody_length
        self.random_seed = random_seed
        self.transitions = defaultdict(lambda: defaultdict(int))
        self.notes = []
        self.time_signature = None
        self.key_signature = None
        self.tempo = None
        self.instrument = None

        if random_seed is not None:
            random.seed(random_seed)

    def load_midi(self):
        # Load the MIDI file and extract notes, durations, and velocities
        score = music21.converter.parse(self.midi_file)
        self.time_signature = score.flat.getElementsByClass(music21.meter.TimeSignature)[0]
        self.key_signature = score.flat.getElementsByClass(music21.key.KeySignature)[0] if score.flat.getElementsByClass(music21.key.KeySignature) else None
        self.tempo = score.flat.getElementsByClass(music21.tempo.MetronomeMark)[0] if score.flat.getElementsByClass(music21.tempo.MetronomeMark) else None

        notes = []
        for part in score.parts:
            for note in part.flat.notesAndRests:
                if isinstance(note, music21.note.Note):
                    notes.append((str(note.pitch), note.quarterLength, note.volume.velocity))
                elif isinstance(note, music21.chord.Chord):
                    notes.append(('.'.join(str(n) for n in note.pitches), note.quarterLength, note.volume.velocity))
                elif isinstance(note, music21.note.Rest):
                    notes.append(('rest', note.quarterLength, 0))
        self.notes = self.cut_leading_rests(notes)

    def cut_leading_rests(self, notes):
        # Remove leading rests from the notes
        for i, note_info in enumerate(notes):
            if note_info[0] != 'rest':
                return notes[i:]
        return notes

    def build_markov_chain(self):
        # Build the Markov chain transitions
        for i in range(len(self.notes) - self.state_size):
            state = tuple(self.notes[i:i + self.state_size])
            next_note = self.notes[i + self.state_size]
            self.transitions[state][next_note] += 1

        # Convert counts to probabilities
        for state, next_notes in self.transitions.items():
            total = sum(next_notes.values())
            for next_note in next_notes:
                self.transitions[state][next_note] /= total

    def generate_melody(self):
        # Generate a new melody using the Markov chain
        # Choose a start state similar to the start state of the original melody
        original_start_state = tuple(self.notes[:self.state_size])
        possible_starts = [state for state in self.transitions.keys() if state[0] == original_start_state[0]]
        if not possible_starts:
            possible_starts = list(self.transitions.keys())
        start_state = random.choice(possible_starts)
        generated_melody = list(start_state)

        current_state = start_state
        for _ in range(self.melody_length - self.state_size):
            next_note_probs = self.transitions[current_state]
            if next_note_probs:
                next_note = random.choices(list(next_note_probs.keys()), list(next_note_probs.values()))[0]
                generated_melody.append(next_note)
                current_state = tuple(generated_melody[-self.state_size:])
            else:
                # If there are no transitions from the current state, choose a new random state
                current_state = random.choice(possible_starts)

        return generated_melody

    def save_melody(self, generated_melody, output_path, file_format='midi', instrument=None):
        # Save the generated melody as a MIDI file
        stream = music21.stream.Stream()
        if self.time_signature:
            stream.append(self.time_signature)
        if self.key_signature:
            stream.append(self.key_signature)
        if self.tempo:
            stream.append(self.tempo)
        if instrument:
            stream.append(instrument)

        for note_info in generated_melody:
            note, duration, velocity = note_info
            if note == 'rest':
                rest = music21.note.Rest(quarterLength=duration)
                stream.append(rest)
            elif '.' in note:
                chord_notes = note.split('.')
                chord = music21.chord.Chord(chord_notes)
                chord.quarterLength = duration
                chord.volume.velocity = velocity
                stream.append(chord)
            else:
                note_obj = music21.note.Note(note)
                note_obj.quarterLength = duration
                note_obj.volume.velocity = velocity
                stream.append(note_obj)
        stream.write(file_format, fp=output_path)

    def plot_melody(self, melody, title):
        # Plot the melody
        pitches = []
        for note_info in melody:
            note, duration, velocity = note_info
            if note == 'rest':
                pitches.append(0)  # Use 0 for rests
            elif '.' in note:
                chord_notes = note.split('.')
                pitches.extend([music21.note.Note(n).pitch.midi for n in chord_notes])
            else:
                pitches.append(music21.note.Note(note).pitch.midi)

        plt.figure(figsize=(10, 4))
        plt.plot(pitches, marker='o', linestyle='-')
        plt.title(title)
        plt.xlabel('Note Index')
        plt.ylabel('MIDI Pitch')
        plt.grid(True)
        plt.show()

    def print_melody(self, melody, title):
        # Print the melody
        print(f"{title}:")
        for i, note_info in enumerate(melody):
            note, duration, velocity = note_info
            print(f"Note {i}: {note} (Duration: {duration}, Velocity: {velocity})")
        print()

if __name__ == "__main__":
    # Parameters
    midi_file = '/kaggle/input/audiofilespcg-v1/Click Clock Wood - Autumn.mid'
    state_size = 20  # Reasonable state size for better context
    melody_length = 4000
    random_seed = 42
    output_path = 'generated_melody.mid'
    file_format = 'midi'
    # instrument = music21.instrument.Violin()  # Set the instrument here

    # Initialize and run the generator
    generator = MarkovMelodyGenerator(midi_file, state_size, melody_length, random_seed)
    generator.load_midi()
    generator.build_markov_chain()
    generated_melody = generator.generate_melody()
    # generator.save_melody(generated_melody, output_path, file_format, instrument)
    generator.save_melody(generated_melody, output_path, file_format)

    print(f"Generated melody saved to {output_path}")

    # Print the original and generated melodies
    generator.print_melody(generator.notes, 'Original Melody')
    generator.print_melody(generated_melody, 'Generated Melody')

    # Plot the original and generated melodies
    generator.plot_melody(generator.notes, 'Original Melody')
    generator.plot_melody(generated_melody, 'Generated Melody')